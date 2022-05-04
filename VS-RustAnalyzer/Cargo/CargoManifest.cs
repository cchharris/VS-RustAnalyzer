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

        private static string TargetPathForBin(string profile, string bin)
        {
            return $"target\\{MapProfileToTargetFolderName(profile)}\\{bin}.exe";
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

            }
            if (Toml.Package.AutoTests)
            {

            }
            if (Toml.Package.AutoBenches)
            {

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

            CargoTarget CargoTargetFor(string name, string path) => new CargoTarget($"{name}.exe", path, _path, (profile) => TargetPathForBin(profile, name), TargetType.Binary, CrateType.Binary);

            if(File.Exists(defaultMain))
            {
                yield return CargoTargetFor(PackageName, defaultMain);
            }
            var binPath = Path.Combine(srcPath, "bin");
            if (Directory.Exists(binPath))
            {
                var files = Directory.EnumerateFiles(binPath, "*.rs");
                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    yield return CargoTargetFor(fileName, file);
                }

                foreach (var dir in EnumerateDirectoriesContainingMain(binPath))
                {
                    var exeName = new DirectoryInfo(dir).Name;
                    yield return CargoTargetFor(exeName, Path.Combine(dir, "main.rs"));
                }
            }
        }
    }
}
