using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlet;
using Tomlet.Models;
using System.IO;
using VS_RustAnalyzer.Cargo.Toml;

namespace VS_RustAnalyzer.Cargo
{
    internal class CargoManifest : ICargoManifest
    {
        private string _path;
        private CargoToml _cachedDoc;

        public const string ProfileDev = "dev";
        public const string ProfileRelease = "release";
        public const string ProfileTest = "test";
        public const string ProfileBench = "bench";

        public static string[] DefaultProfiles = new [] { ProfileDev, ProfileRelease, ProfileTest, ProfileBench};

        private CargoToml Toml { get {
                if (_cachedDoc == null)
                {
                    _cachedDoc = new CargoToml(TomlParser.ParseFile(_path));
                }
                return _cachedDoc;
            } }

        public CargoManifest(string path)
        {
            this._path = path;
        }

        public void ClearCache() { _cachedDoc = null; }

        public string PackageName => Toml.Package.Name;

        public List<ICargoTarget> Targets => throw new NotImplementedException();

        // TODO
        public IEnumerable<string> Profiles => DefaultProfiles;

        public IEnumerable<string> BinTargetPaths(string profile)
        {
            foreach(var bin in EnumerateFileSystemInferredBins())
            {
                yield return bin.TargetPath(profile);
            }
        }

        public static string MapProfileToTargetFolderName(string profile)
        {
            switch(profile)
            {
                case ProfileDev:
                case ProfileTest:
                    return "debug";
                case ProfileRelease:
                case ProfileBench:
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

        public IEnumerable<ICargoTarget> EnumerateFileSystemInferredTargets()
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

        public IEnumerable<ICargoTarget> EnumerateFileSystemInferredBins()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_path);
            var srcPath = Path.Combine(directoryContainingCargo, "src");
            var defaultMain = Path.Combine(srcPath, "main.rs");

            if(File.Exists(defaultMain))
            {
                yield return CargoTargetFor(PackageName, defaultMain, string.Empty, TargetType.Binary, CrateType.Binary);
            }
            var binPath = Path.Combine(srcPath, "bin");
            foreach(var target in EnumerateFileSystemForInferredTargets(binPath, string.Empty, TargetType.Binary, CrateType.Binary))
            {
                yield return target;
            }
        }

        public IEnumerable<ICargoTarget> EnumerateFileSystemInferredBenches()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_path);
            var benchesPath = Path.Combine(directoryContainingCargo, "benches");
            foreach (var target in EnumerateFileSystemForInferredTargets(benchesPath, string.Empty, TargetType.Bench, CrateType.Bench))
            {
                yield return target;
            }
        }

        public IEnumerable<ICargoTarget> EnumerateFileSystemInferredExamples()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_path);
            var examplesPath = Path.Combine(directoryContainingCargo, "examples");
            foreach (var target in EnumerateFileSystemForInferredTargets(examplesPath, "examples", TargetType.Example, CrateType.Binary))
            {
                yield return target;
            }
        }

        public IEnumerable<ICargoTarget> EnumerateFileSystemInferredTests()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_path);
            var testsPath = Path.Combine(directoryContainingCargo, "tests");
            foreach (var target in EnumerateFileSystemForInferredTargets(testsPath, string.Empty, TargetType.Test, CrateType.Test))
            {
                yield return target;
            }
        }


        private IEnumerable<ICargoTarget> EnumerateFileSystemForInferredTargets(string dir, string extra, TargetType type, CrateType crate)
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
            return new CargoTarget($"{name}.exe", path, _path, (profile) => TargetPathForBin(profile, name, extra), type, crate);
        }
    }
}
