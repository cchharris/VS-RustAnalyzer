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

        public string PackageName => _cachedDoc.Package.Name;

        public List<ICargoTarget> Targets => throw new NotImplementedException();
    }
}
