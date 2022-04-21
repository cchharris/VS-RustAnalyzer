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
        public CargoPackageToml(TomlTable table)
        {
            this._table = table;
        }

        public string Name => _table.GetString(_name);
    }
}
