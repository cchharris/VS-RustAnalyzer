using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer.Cargo
{
    public class CargoTargetInferred
    {
        public string Name { get; set; }
        public string SrcPath { get; set; }
        public string CratePath { get; set; }
        public TargetType TargetType { get; set; }
        public CrateType CrateType { get; set; }

        public CargoTargetInferred(string name, string srcPath, string cratePath, TargetType type, CrateType crate)
        {
            Name = name;
            SrcPath = srcPath;
            CratePath = cratePath;
            TargetType = type;
            CrateType = crate;
        }
    }
}
