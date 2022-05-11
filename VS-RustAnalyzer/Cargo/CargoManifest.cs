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
            _targets.LoadTargets();
        }

        public void ClearCache() {
            _cachedDoc = null;
            _targets.ClearCache();
        }

        public string PackageName => Toml.Package.Name;

        public List<ICargoTarget> Targets => throw new NotImplementedException();

        // TODO
        public IEnumerable<string> Profiles => DefaultProfiles;

        public IEnumerable<string> BinTargetPaths(string profile)
        {
            _targets.LoadTargets();
            foreach (var bin in _targets.EnumerateTargetsByType(TargetType.Binary))
            {
                foreach(var target in bin.TargetPath(profile))
                {
                    yield return target;
                }
            }
        }

        public IEnumerable<ICargoTarget> EnumerateTargetsByType(TargetType type)
        {
            return _targets.EnumerateTargetsByType(type);
        }
    }
}
