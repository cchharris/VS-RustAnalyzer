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

        public string Name => IsDefined ? Defined.Name : Inferred.Name;

        public string SrcPath => IsDefined ? Defined.Path : Inferred.SrcPath;

        public string ManifestPath => _manifestPath;

        public TargetForProfileDelegate TargetPath => throw new NotImplementedException();

        public TargetType TargetType => IsDefined ? Defined.Type : Inferred.TargetType;

        public IEnumerable<CrateType> CrateType => throw new NotImplementedException();

        public bool Test => throw new NotImplementedException();

        public bool DocTest => throw new NotImplementedException();

        public bool Bench => throw new NotImplementedException();

        public bool Doc => throw new NotImplementedException();

        public bool Plugin => throw new NotImplementedException();

        public bool ProcMacro => throw new NotImplementedException();

        public bool Harness => throw new NotImplementedException();

        public string Edition => throw new NotImplementedException();

        public IEnumerable<string> RequiredFeatures => throw new NotImplementedException();

        private CargoTargetInferred Inferred { get; }
        private CargoTargetToml Defined { get; }
    }
}
