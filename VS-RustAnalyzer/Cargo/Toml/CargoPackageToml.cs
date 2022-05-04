using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlet.Models;

namespace VS_RustAnalyzer.Cargo.Toml
{
    internal class CargoPackageToml
    {
        private TomlTable _table;
        private const string _name = "name";
        private const string _autobins = "autobins";
        private const string _autoexamples = "autoexamples";
        private const string _autotests = "autotests";
        private const string _autobenches = "autobenches";
        public CargoPackageToml(TomlTable table)
        {
            this._table = table;
        }

        public string Name => _table.GetString(_name);
        public bool AutoBins => _table.ContainsKey(_autobins) ? _table.GetBoolean(_autobins) : true;
        public bool AutoExamples => _table.ContainsKey(_autoexamples) ? _table.GetBoolean(_autoexamples) : true;
        public bool AutoTests => _table.ContainsKey(_autotests) ? _table.GetBoolean(_autotests) : true;
        public bool AutoBenches => _table.ContainsKey(_autobenches) ? _table.GetBoolean(_autobenches) : true;
    }
}
