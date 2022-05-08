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
        private string _manifestPath;
        public CargoTarget(string manifestPath, CargoTargetInferred inferred, CargoTargetToml defined)
        {
            Inferred=inferred;
            Defined=defined;
            _manifestPath=manifestPath;
        }

        public bool IsInferred => Inferred != null;
        public bool IsDefined => Defined != null;

        // All of these are wrong - need to check if Defined has the individual field defined as well

        public string Name => IsDefined ? Defined.Name : Inferred.Name;

        public string SrcPath => IsDefined ? Defined.Path : Inferred.SrcPath;

        public string ManifestPath => _manifestPath;

        public TargetForProfileDelegate TargetPath => throw new NotImplementedException();

        public TargetType TargetType => IsDefined ? Defined.Type : Inferred.TargetType;

        public IEnumerable<CrateType> CrateType => IsDefined ? Defined.CrateTypes : new CrateType[] { Inferred.CrateType };

        public bool Test => IsDefined ? Defined.Test : Inferred.Test;

        public bool DocTest => IsDefined ? Defined.DocTest : Inferred.DocTest;

        public bool Bench => IsDefined ? Defined.Bench : Inferred.Bench;

        public bool Doc => IsDefined ? Defined.Doc : Inferred.Doc;

        public bool Plugin => IsDefined ? Defined.Plugin : Inferred.Plugin;

        public bool ProcMacro => IsDefined ? Defined.ProcMacro : Inferred.ProcMacro;

        public bool Harness => IsDefined ? Defined.Harness : Inferred.Harness;

        public string Edition => IsDefined ? Defined.Edition : Inferred.Edition;

        public IEnumerable<string> RequiredFeatures => IsDefined ? Defined.RequiredFeatures : Inferred.RequiredFeatures;

        private CargoTargetInferred Inferred { get; }
        private CargoTargetToml Defined { get; }
    }
}
