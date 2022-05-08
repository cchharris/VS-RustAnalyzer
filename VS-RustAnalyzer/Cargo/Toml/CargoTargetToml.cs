using Tomlet.Models;
using System.Linq;
using System.Collections.Generic;
using System;

namespace VS_RustAnalyzer.Cargo.Toml
{
    /**
     * https://doc.rust-lang.org/cargo/reference/cargo-targets.html#configuring-a-target
    */
    public class CargoTargetToml
    {
        private TomlTable _tomlTable;
        private readonly TargetType _type;

        /**
name = "foo"           # The name of the target.
path = "src/lib.rs"    # The source file of the target.
test = true            # Is tested by default.
doctest = true         # Documentation examples are tested by default.
bench = true           # Is benchmarked by default.
doc = true             # Is documented by default.
plugin = false         # Used as a compiler plugin (deprecated).
proc-macro = false     # Set to `true` for a proc-macro library.
harness = true         # Use libtest harness.
edition = "2015"       # The edition of the target.
crate-type = ["lib"]   # The crate types to generate.
required-features = [] # Features required to build this target (N/A for lib).
*/
        private const string _name = "name";
        private const string _path = "path";
        private const string _test = "test";
        private const string _doctest = "doctest";
        private const string _bench = "bench";
        private const string _doc = "doc";
        private const string _plugin = "plugin";
        private const string _proc_macro = "proc-macro";
        private const string _harness = "harness";
        private const string _edition = "edition";
        private const string _crate_type = "crate-type";
        private const string _required_features = "required-features";

        public string Name => NameDefined ? _tomlTable.GetString(_name) : string.Empty;
        public bool NameDefined => _tomlTable.ContainsKey(_name);
        public string Path => PathDefined ? _tomlTable.GetString(_path) : string.Empty;
        public bool PathDefined => _tomlTable.ContainsKey(_path);
        public bool Test => TestDefined ? _tomlTable.GetBoolean(_test) : _type != TargetType.Example;
        public bool TestDefined => _tomlTable.ContainsKey(_test);
        public bool DocTest => DocTestDefined ? _tomlTable.GetBoolean(_doctest) : _type == TargetType.Library;
        public bool DocTestDefined => _tomlTable.ContainsKey (_doctest);
        public bool Bench
        {
            get
            {
                if (BenchDefined) return _tomlTable.GetBoolean(_bench);
                switch (_type)
                {
                    case TargetType.Library:
                    case TargetType.Binary:
                    case TargetType.Bench:
                        return true;
                    default: return false;
                }
            }
        }
        public bool BenchDefined => _tomlTable.ContainsKey(_bench);

        public bool Doc => DocDefined ? _tomlTable.GetBoolean(_doc) : (_type == TargetType.Library || _type == TargetType.Binary);
        public bool DocDefined => _tomlTable.ContainsKey(_doc);
        public bool Plugin => PluginDefined ? _tomlTable.GetBoolean(_plugin) : false;
        public bool PluginDefined => _tomlTable.ContainsKey(_plugin);
        public bool ProcMacro => ProcMacroDefined ? _tomlTable.GetBoolean(_proc_macro) : false;
        public bool ProcMacroDefined => _tomlTable.ContainsKey(_proc_macro);
        public bool Harness => HarnessDefined ? _tomlTable.GetBoolean(_harness) : true;
        public bool HarnessDefined => _tomlTable.ContainsKey(_harness);
        public string Edition => EditionDefined ? _tomlTable.GetString(_edition) : string.Empty;
        public bool EditionDefined => _tomlTable.ContainsKey(_edition);
        public IEnumerable<CrateType> CrateTypes
        {
            get
            {
                if (CrateTypesDefined)
                {
                    var tomlCrates = _tomlTable.GetArray(_crate_type);
                    foreach (var crate in tomlCrates)
                    {
                        if (Enum.TryParse(crate.StringValue, out CrateType type))
                        {
                            yield return type;
                        }
                    }
                }
                else switch (Type)
                {
                    case TargetType.Bench: yield return CrateType.Bench; break;
                    case TargetType.Binary: yield return CrateType.Binary; break;
                    case TargetType.Example: yield return CrateType.Binary; break;
                    case TargetType.Library: yield return CrateType.RustLibrary; break;
                    case TargetType.Test: yield return CrateType.Test; break;
                    default: yield break;
                }
            }
        }
        public bool CrateTypesDefined => _tomlTable.ContainsKey(_crate_type);
        public IEnumerable<string> RequiredFeatures
        {
            get
            {
                if (!RequiredFeaturesDefined)
                    return Enumerable.Empty<string>();
                return _tomlTable.GetArray(_required_features).ArrayValues.Select(x => x.StringValue);
            }
        }
        public bool RequiredFeaturesDefined => _tomlTable.ContainsKey(_required_features); 

        public TargetType Type => _type;

        public CargoTargetToml(TomlTable tomlTable, TargetType type)
        {
            this._tomlTable = tomlTable;
            this._type=type;
        }
    }
}