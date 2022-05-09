using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlet.Models;

namespace VS_RustAnalyzer.Cargo.Toml
{
    /**
     *  From https://doc.rust-lang.org/cargo/reference/manifest.html
     */
    public class CargoToml
    {
        // Incomplete.  Will fill in as needed.

        private TomlDocument _document;
        private const string _cargo_features = "cargo-features";
        private const string _subtable_package = "package";
        private const string _subtable_lib = "lib";
        private const string _subtable_bin = "bin";
        private const string _subtable_example = "example";
        private const string _subtable_test = "test";
        private const string _subtable_bench = "bench";
        private const string _subtable_dependencies = "dependencies";
        private const string _subtable_dev_dependencies = "dev-dependencies";
        private const string _subtable_build_dependencies = "build-dependencies";
        private const string _subtable_target = "target";
        private const string _subtable_features = "features";
        private const string _subtable_patch = "patch";
        private const string _subtable_replace = "replace";
        private const string _subtable_profile = "profile";
        private const string _subtable_workspace = "workspace";

        public CargoToml(TomlDocument document)
        {
            this._document = document;
        }

        public CargoPackageToml Package => new CargoPackageToml(_document.GetSubTable(_subtable_package));
        // 0-1 Lib, 0+ Bin, Example, Test, Bench
        public CargoTargetToml Lib => _document.ContainsKey(_subtable_lib) ? new CargoTargetToml(_document.GetSubTable(_subtable_lib), TargetType.Library, Package.Name) : null;
        public IEnumerable<CargoTargetToml> Bins => TablesFromKey(_subtable_bin).Select(t => new CargoTargetToml(t, TargetType.Binary, string.Empty));
        public IEnumerable<CargoTargetToml> Examples => TablesFromKey(_subtable_example).Select(t => new CargoTargetToml(t, TargetType.Example, string.Empty));
        public IEnumerable<CargoTargetToml> Tests => TablesFromKey(_subtable_test).Select(t => new CargoTargetToml(t, TargetType.Test, string.Empty));
        public IEnumerable<CargoTargetToml> Benches => TablesFromKey(_subtable_bench).Select(t => new CargoTargetToml(t, TargetType.Bench, string.Empty));

        public IEnumerable<CargoTargetToml> AllTargets { get
            {
                var lib = Lib;
                IEnumerable<CargoTargetToml> libEnum = Lib == null ? Enumerable.Empty<CargoTargetToml>() : new CargoTargetToml[] { lib };
                return libEnum.Concat(Bins).Concat(Examples).Concat(Tests).Concat(Benches);
            }
        }

        private IEnumerable<TomlTable> TablesFromKey(string key)
        {
            if (!_document.ContainsKey(key))
                return Enumerable.Empty<TomlTable>();
            return _document.GetArray(key).Where(t => t is TomlTable).Select(t => t as TomlTable);
        }
    }
}
