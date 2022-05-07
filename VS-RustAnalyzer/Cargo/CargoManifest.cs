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
    public class CargoManifest : ICargoManifest
    {
        private string _path;
        private CargoToml _cachedDoc;
        private CargoTargets _targets;

        public const string ProfileDev = "dev";
        public const string ProfileRelease = "release";
        public const string ProfileTest = "test";
        public const string ProfileBench = "bench";

        public static string[] DefaultProfiles = new[] { ProfileDev, ProfileRelease, ProfileTest, ProfileBench };

        public CargoToml Toml
        {
            get
            {
                if (_cachedDoc == null)
                {
                    _cachedDoc = new CargoToml(TomlParser.ParseFile(_path));
                }
                return _cachedDoc;
            }
        }

        public string Path => _path;

        public CargoManifest(string path)
        {
            this._path = path;
            _targets = new CargoTargets(this);
        }

        public void ClearCache() { _cachedDoc = null; }

        public string PackageName => Toml.Package.Name;

        public List<ICargoTarget> Targets => throw new NotImplementedException();

        // TODO
        public IEnumerable<string> Profiles => DefaultProfiles;

        public IEnumerable<string> BinTargetPaths(string profile)
        {
            foreach (var bin in _targets.EnumerateFileSystemInferredBins())
            {
                yield return bin.TargetPath(profile);
            }
        }
    }
}
