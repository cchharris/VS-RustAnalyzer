using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlet;
using Tomlet.Models;
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
            yield return $"target\\{MapProfileToTargetFolderName(profile)}\\{PackageName}.exe";
        }

        public string MapProfileToTargetFolderName(string profile)
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
    }
}
