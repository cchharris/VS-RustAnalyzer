using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VS_RustAnalyzer.Cargo.Toml;

namespace VS_RustAnalyzer.Cargo
{
    internal class CargoTargets
    {
        private CargoManifest _manifest;
        public CargoTargets(CargoManifest manifest)
        {
            _manifest = manifest;
        }

        private CargoToml Toml => _manifest.Toml;

        private Dictionary<string, CargoTarget> _targets = new Dictionary<string, CargoTarget>();
        private Dictionary<TargetType, List<CargoTarget>> _targetsByType = new Dictionary<TargetType, List<CargoTarget>>();
        bool _loaded = false;

        public void ClearCache()
        {
            _targets.Clear();       
            _targetsByType.Clear();
            _loaded = false;
        }

        public static string MapProfileToTargetFolderName(string profile)
        {
            switch(profile)
            {
                case CargoManifest.ProfileDev:
                case CargoManifest.ProfileTest:
                    return "debug";
                case CargoManifest.ProfileRelease:
                case CargoManifest.ProfileBench:
                    return "release";
                default: // Custom profile
                    return profile;
                   
            }
        }
        private static string TargetPathForBin(string profile, string bin, string extra = "")
        {
            if (string.IsNullOrEmpty(extra))
                return $"target\\{MapProfileToTargetFolderName(profile)}\\{bin}.exe";
            return $"target\\{MapProfileToTargetFolderName(profile)}\\{extra}\\{bin}.exe";
        }

        public void LoadTargets()
        {
            if (_loaded) return;

            var fileSystemInferred = EnumerateAllFileSystemInferredTargets();
            foreach(var target in fileSystemInferred)
            {
                _targets[target.Name] = target;
                if(!_targetsByType.TryGetValue(target.TargetType, out List<CargoTarget> targets))
                {
                    targets = new List<CargoTarget>();
                    _targetsByType[target.TargetType] = targets;
                }
                targets.Add(target);
            }

            // Load from TOML
            _loaded = true;
        }

        public IEnumerable<ICargoTarget> EnumerateTargetsByType(TargetType type)
        {
            if (_targetsByType.TryGetValue(type, out List<CargoTarget> targets))
                return targets;
            return Enumerable.Empty<ICargoTarget>();
        }

        private IEnumerable<CargoTarget> EnumerateAllFileSystemInferredTargets()
        {
            if(Toml.Package.AutoBins)
            {
                foreach(var target in EnumerateFileSystemInferredBins())
                {
                    yield return target;
                }
            }

            if (Toml.Package.AutoExamples)
            {
                foreach(var target in EnumerateFileSystemInferredExamples())
                {
                    yield return target;
                }
            }
            if (Toml.Package.AutoTests)
            {
                foreach(var target in EnumerateFileSystemInferredTests())
                {
                    yield return target;
                }
            }
            if (Toml.Package.AutoBenches)
            {
                foreach(var target in EnumerateFileSystemInferredBenches())
                {
                    yield return target;
                }
            }
        }

        private IEnumerable<string> EnumerateDirectoriesContainingMain(string path)
        {
            var dirs = Directory.EnumerateDirectories(path);
            foreach(var dir in dirs)
            {
                var mainPath = Path.Combine(dir, "main.rs");
                if (File.Exists(mainPath))
                {
                    yield return dir;
                }
            }
        }

        private IEnumerable<CargoTarget> EnumerateFileSystemInferredBins()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var srcPath = Path.Combine(directoryContainingCargo, "src");
            var defaultMain = Path.Combine(srcPath, "main.rs");

            if(File.Exists(defaultMain))
            {
                yield return CargoTargetFor(PackageNameAsTarget(), defaultMain, string.Empty, TargetType.Binary, CrateType.Binary);
            }
            var binPath = Path.Combine(srcPath, "bin");
            foreach(var target in EnumerateFileSystemForInferredTargets(binPath, string.Empty, TargetType.Binary, CrateType.Binary))
            {
                yield return target;
            }
        }

        private CargoTarget FileSystemInferredLib()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var srcPath = Path.Combine(directoryContainingCargo, "src");
            var defaultLib = Path.Combine(srcPath, "lib.rs");
            if(File.Exists(defaultLib))
            {
                return CargoTargetFor(PackageNameAsTarget(), defaultLib, string.Empty, TargetType.Library, CrateType.RustLibrary);
            }
            return null;
        }

        private IEnumerable<CargoTarget> EnumerateFileSystemInferredBenches()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var benchesPath = Path.Combine(directoryContainingCargo, "benches");
            foreach (var target in EnumerateFileSystemForInferredTargets(benchesPath, string.Empty, TargetType.Bench, CrateType.Bench))
            {
                yield return target;
            }
        }

        private IEnumerable<CargoTarget> EnumerateFileSystemInferredExamples()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var examplesPath = Path.Combine(directoryContainingCargo, "examples");
            foreach (var target in EnumerateFileSystemForInferredTargets(examplesPath, "examples", TargetType.Example, CrateType.Binary))
            {
                yield return target;
            }
        }

        private IEnumerable<CargoTarget> EnumerateFileSystemInferredTests()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var testsPath = Path.Combine(directoryContainingCargo, "tests");
            foreach (var target in EnumerateFileSystemForInferredTargets(testsPath, string.Empty, TargetType.Test, CrateType.Test))
            {
                yield return target;
            }
        }


        private IEnumerable<CargoTarget> EnumerateFileSystemForInferredTargets(string dir, string extra, TargetType type, CrateType crate)
        {
            if (Directory.Exists(dir))
            {
                var files = Directory.EnumerateFiles(dir, "*.rs");
                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    yield return CargoTargetFor(fileName, file, extra, type, crate);
                }

                foreach (var d in EnumerateDirectoriesContainingMain(dir))
                {
                    var exeName = new DirectoryInfo(d).Name;
                    yield return CargoTargetFor(exeName, Path.Combine(d, "main.rs"), extra, type, crate);
                }
            }
        }

        private CargoTarget CargoTargetFor(string name, string path, string extra, TargetType type, CrateType crate)
        {
            return new CargoTarget($"{name}.exe", path, _manifest.Path, (profile) => TargetPathForBin(profile, name, extra), type, crate);
        }

        private string PackageNameAsTarget()
        {
            return _manifest.PackageName.Replace('-', '_');
        }
        
    }
}
