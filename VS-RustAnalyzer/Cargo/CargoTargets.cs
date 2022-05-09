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

        private Dictionary<string, CargoTarget> _targetsByName = new Dictionary<string, CargoTarget>();
        private Dictionary<string, CargoTarget> _targetsByPath = new Dictionary<string, CargoTarget>();
        private Dictionary<TargetType, List<CargoTarget>> _targetsByType = new Dictionary<TargetType, List<CargoTarget>>();
        bool _loaded = false;

        public void ClearCache()
        {
            _targetsByName.Clear();       
            _targetsByPath.Clear();       
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

        /**
         * Example 
         *      src/
         *          bin/
         *             multifile/
         *                 main.rs
         *             singlefile.rs
         *
         * Toml
         * [[bin]]
         *  name = multifile
         *  path = src/bin/singlefile.rs
         * 
         * A target with name singlefile doesn't exist
         * A target with multifile exists, consisting of singlefile.rs
         * src/bin/multifile/main.rs does not get compiled into a target
         *
         * If path isnt' set, multifile has a failing test, and test=false, the test won't run when we run `cargo test`,
         * so we can obviously also map to 
         *
         * Our algorithm:
         *  Load all toml targets
         *  Load all inferred targets, whose names aren't already taken by a toml target
         *  Map them to Toml based on Path
         *   If Path isn't defined in Toml, map based on name
         *
         */
        public void LoadTargets()
        {
            if (_loaded) return;

            // Load from TOML
            List<CargoTargetToml> definedTargets = Toml.AllTargets.ToList();
            foreach(CargoTargetToml tt in definedTargets)
            {
                CargoTargetInferred inferred = null;
            }

            List<CargoTargetInferred> inferredTargets = EnumerateAllFileSystemInferredTargets().ToList();
            ILookup<string, CargoTargetInferred> InferredTargetsByPath = inferredTargets.ToLookup((target) => target.SrcPath);
            ILookup<string, CargoTargetInferred> InferredTargetsByName = InferredTargetsByPath = inferredTargets.ToLookup(target => target.Name);


            //_targetsByName[target.Name] = target;
            //targets.Add(target);
            /*
                if(!_targetsByType.TryGetValue(target.TargetType, out List<CargoTargetInferred> targets))
                {
                    targets = new List<CargoTargetInferred>();
                    _targetsByType[target.TargetType] = targets;
                }
            */

            _loaded = true;
        }

        public IEnumerable<ICargoTarget> EnumerateTargetsByType(TargetType type)
        {
            if (_targetsByType.TryGetValue(type, out List<CargoTarget> targets))
                return targets;
            return Enumerable.Empty<ICargoTarget>();
        }

        private IEnumerable<CargoTargetInferred> EnumerateAllFileSystemInferredTargets()
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
            return Directory.EnumerateDirectories(path).Where(DirectoryContainsMain);
        }

        private bool DirectoryContainsMain(string dir)
        {
            return File.Exists(Path.Combine(dir, "main.rs"));
        }

        private IEnumerable<CargoTargetInferred> EnumerateFileSystemInferredBins()
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

        private CargoTargetInferred FileSystemInferredLib()
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

        private IEnumerable<CargoTargetInferred> EnumerateFileSystemInferredBenches()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var benchesPath = Path.Combine(directoryContainingCargo, "benches");
            foreach (var target in EnumerateFileSystemForInferredTargets(benchesPath, string.Empty, TargetType.Bench, CrateType.Bench))
            {
                yield return target;
            }
        }

        private IEnumerable<CargoTargetInferred> EnumerateFileSystemInferredExamples()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var examplesPath = Path.Combine(directoryContainingCargo, "examples");
            foreach (var target in EnumerateFileSystemForInferredTargets(examplesPath, "examples", TargetType.Example, CrateType.Binary))
            {
                yield return target;
            }
        }

        private IEnumerable<CargoTargetInferred> EnumerateFileSystemInferredTests()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var testsPath = Path.Combine(directoryContainingCargo, "tests");
            foreach (var target in EnumerateFileSystemForInferredTargets(testsPath, string.Empty, TargetType.Test, CrateType.Test))
            {
                yield return target;
            }
        }


        private IEnumerable<CargoTargetInferred> EnumerateFileSystemForInferredTargets(string dir, string extra, TargetType type, CrateType crate)
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

        private CargoTargetInferred CargoTargetFor(string name, string path, string extra, TargetType type, CrateType crate)
        {
            return new CargoTargetInferred(name, path, _manifest.Path, (profile) => TargetPathForBin(profile, name, extra), type, crate);
        }

        private string PackageNameAsTarget()
        {
            return _manifest.PackageName.Replace('-', '_');
        }
        
        public string InferPathFromName(string name, TargetType type, bool isDefault)
        {
            switch (type)
            {
                case TargetType.Library:
                        return Path.Combine("src", "lib.rs");
                case TargetType.Test:
                    return InferFilePathFromName(name, Path.Combine(_manifest.Path, "tests"), "test");
                case TargetType.Example:
                    return InferFilePathFromName(name, Path.Combine(_manifest.Path, "examples"), "examples");
                case TargetType.Bench:
                    return InferFilePathFromName(name, Path.Combine(_manifest.Path, "benches"), "benches");
                case TargetType.Binary:
                    if (isDefault)
                        return Path.Combine("src", "main.rs");
                    return InferFilePathFromName(name, Path.Combine(_manifest.Path, "src", "bin"), Path.Combine("src", "bin"));
                default: throw new InvalidOperationException("Inferring path from an unknown TargetType");
            }
        }

        private string InferFilePathFromName(string name, string fullParentPath, string relativeFromCargoPath)
        {
            if (File.Exists(Path.Combine(fullParentPath, $"{name}.rs")))
                return Path.Combine(relativeFromCargoPath, $"{name}.rs");
            if (DirectoryContainsMain(Path.Combine(fullParentPath, name)))
                return Path.Combine(relativeFromCargoPath, name, "main.rs");
            return null;
        }
    }
}
