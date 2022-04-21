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
    internal class CargoToml
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
    }
}
