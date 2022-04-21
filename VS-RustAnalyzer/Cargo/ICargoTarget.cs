using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer.Cargo
{
    enum TargetType
    {
        Library, // lib
        Binary, // bin
        Example, // example
        Test, // test
        Bench // bench
    }

    enum CrateType
    {
        Binary, // bin
        Library, // lib
        RustLibrary, // rlib
        DynamicLibrary, // dylib
        CDynamicLibrary, // cdylib
        StaticLibrary, // staticlib
        Macro, // proc-macro
    }

    internal interface ICargoTarget
    {
    }
}
