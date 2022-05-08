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
        public TargetForProfileDelegate TargetPath { get; set; }
        public TargetType TargetType { get; set; }
        public CrateType CrateType { get; set; }
    }
}
