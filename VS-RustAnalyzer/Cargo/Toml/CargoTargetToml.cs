using Tomlet.Models;
using System.Linq;
using System.Collections.Generic;

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

        public string Name => _tomlTable.ContainsKey(_name) ? _tomlTable.GetString(_name) : string.Empty;
        public string Path => _tomlTable.ContainsKey(_path) ? _tomlTable.GetString(_path) : string.Empty;
        public bool Test => _tomlTable.ContainsKey(_test) ? _tomlTable.GetBoolean(_test) : _type != TargetType.Example;
        public bool DocTest => _tomlTable.ContainsKey(_doctest) ? _tomlTable.GetBoolean(_doctest) : _type == TargetType.Library;
        public bool Bench
        {
            get
            {
                if (_tomlTable.ContainsKey(_bench)) return _tomlTable.GetBoolean(_bench);
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

        public bool Doc => _tomlTable.ContainsKey(_doc) ? _tomlTable.GetBoolean(_doc) : (_type == TargetType.Library || _type == TargetType.Binary);
        public bool Plugin => _tomlTable.ContainsKey(_plugin) ? _tomlTable.GetBoolean(_plugin) : false;
        public bool ProcMacro => _tomlTable.ContainsKey(_proc_macro) ? _tomlTable.GetBoolean(_proc_macro) : false;
        public bool Harness => _tomlTable.ContainsKey(_harness) ? _tomlTable.GetBoolean(_harness) : true;
        public string Edition => _tomlTable.ContainsKey(_edition) ? _tomlTable.GetString(_edition) : string.Empty;
        public IEnumerable<string> RequiredFeatures
        {
            get
            {
                if (!_tomlTable.ContainsKey(_required_features))
                    return Enumerable.Empty<string>();
                return _tomlTable.GetArray(_required_features).ArrayValues.Select(x => x.StringValue);
            }
        }

        public TargetType Type => _type;

        public CargoTargetToml(TomlTable tomlTable, TargetType type)
        {
            this._tomlTable = tomlTable;
            this._type=type;
        }
    }
}