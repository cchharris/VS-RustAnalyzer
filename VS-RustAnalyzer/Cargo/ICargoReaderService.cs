using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer.Cargo
{
    internal interface ICargoReaderService 
    {
        ICargoManifest CargoManifestForFile(string path);
    }
}
