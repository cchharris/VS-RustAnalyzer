using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer.Cargo
{
    internal interface ICargoManifest
    {
        string PackageName { get; }

        List<ICargoTarget> Targets { get; }

        IEnumerable<string> Profiles { get; }

        IEnumerable<string> BinTargetPaths(string profile);
    }
}
