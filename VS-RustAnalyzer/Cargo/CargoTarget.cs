using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VS_RustAnalyzer.Cargo.Toml;

namespace VS_RustAnalyzer.Cargo
{
    public class CargoTarget : ICargoTarget
    {
        private readonly string _manifestPath;
        private readonly string _name;
        private readonly string _path;
        private readonly TargetType _type;
        private readonly TargetForProfileDelegate _targetForProfileDelegate;

        public string ManifestPath => _manifestPath;

        public string Name => _name;

        public string SrcPath => _path;

        public TargetForProfileDelegate TargetPath => _targetForProfileDelegate;

        public TargetType TargetType => _type;

        private IEnumerable<CrateType> _crates_override = null;
        public IEnumerable<CrateType> CrateTypes
        {
            get
            {
                if (_crates_override != null)
                    return _crates_override;
                switch (_type)
                {
                    case TargetType.Bench: return new CrateType[] { CrateType.Bench };
                    case TargetType.Binary: return new CrateType[] { CrateType.Binary };
                    case TargetType.Example: return new CrateType[] { CrateType.Binary };
                    case TargetType.Library: return new CrateType[] { CrateType.RustLibrary };
                    case TargetType.Test: return new CrateType[] { CrateType.Test };
                    default: return Enumerable.Empty<CrateType>();
                }
            }
        }

        private bool? _test_override = null;
        public bool Test => _test_override.HasValue ? _test_override.Value : _type != TargetType.Example;

        private bool? _doctest_override = null;
        public bool DocTest => _doctest_override.HasValue ? _doctest_override.Value : _type == TargetType.Library;

        private bool? _bench_override = null;
        public bool Bench { get
            {
                if (_bench_override.HasValue) return _bench_override.Value;
                switch (_type)
                {
                    case TargetType.Library:
                    case TargetType.Binary:
                    case TargetType.Bench:
                        return true;
                    default: return false;
                }
            } }

        private bool? _doc_override = null;
        public bool Doc => _doc_override.HasValue ? _doc_override.Value : (_type == TargetType.Library || _type == TargetType.Binary);

        private bool? _plugin_override = null;
        public bool Plugin => _plugin_override.HasValue ? _plugin_override.Value : false;

        private bool? _procmacro_override = null;
        public bool ProcMacro => _procmacro_override.HasValue ? _procmacro_override.Value : false;

        private bool? _harness_override = null;
        public bool Harness => _harness_override.HasValue ? _harness_override.Value : true;

        private string _edition_override = null;
        public string Edition => _edition_override != null ? _edition_override : string.Empty;

        private IEnumerable<string> _requiredfeatures_override = null;
        public IEnumerable<string> RequiredFeatures => _requiredfeatures_override != null ? _requiredfeatures_override : Enumerable.Empty<string>();

        public CargoTarget(string manifestPath, string name, string path, TargetType type, TargetForProfileDelegate targetForProfileDelegate)
        {
            _manifestPath=manifestPath;
            _name = name;
            _path = path;
            _type = type;
            _targetForProfileDelegate = targetForProfileDelegate;
        }

        public CargoTarget(string manifestPath, string name, string path, TargetType type, TargetForProfileDelegate targetForProfileDelegate, CargoTargetToml toml) :
            this(manifestPath, name, path, type, targetForProfileDelegate)
        {
            // Name, path, Type, already required
            if (toml.BenchDefined)
                _bench_override = toml.Bench;
            if (toml.CrateTypesDefined)
                _crates_override = toml.CrateTypes;
            if (toml.DocDefined)
                _doc_override = toml.Doc;
            if (toml.DocTestDefined)
                _doctest_override = toml.DocTest;
            if (toml.EditionDefined)
                _edition_override = toml.Edition;
            if (toml.HarnessDefined)
                _harness_override = toml.Harness;
            if (toml.PluginDefined)
                _plugin_override = toml.Plugin;
            if (toml.ProcMacroDefined)
                _procmacro_override = toml.ProcMacro;
            if (toml.RequiredFeaturesDefined)
                _requiredfeatures_override = toml.RequiredFeatures;
            if (toml.TestDefined)
                _test_override = toml.Test;
        }
    }
}
