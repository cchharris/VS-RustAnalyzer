using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer.Cargo
{
    public enum TargetType
    {
        Library, // lib
        Binary, // bin
        Example, // example
        Test, // test
        Bench // bench
    }

    public enum CrateType
    {
        // Provided for ease of use - don't actually matter here.
        Binary, // bin
        Bench, // bench
        Test, // test
        // Below are where these matter - Libraries and Examples only
        Library, // lib
        RustLibrary, // rlib
        DynamicLibrary, // dylib
        CDynamicLibrary, // cdylib
        StaticLibrary, // staticlib
        Macro, // proc-macro
    }


    public delegate string TargetForProfileDelegate(string profile);

    public interface ICargoTarget
    {
        string Name { get; }
        string SrcPath { get; }
        string ManifestPath { get; }
        TargetForProfileDelegate TargetPath { get; }
        TargetType TargetType { get; }
        IEnumerable<CrateType> CrateType { get; }
        bool Test { get; }
        bool DocTest { get; }
        bool Bench { get; }
        bool Doc { get; }
        bool Plugin { get; }
        bool ProcMacro { get; }
        bool Harness { get; }
        string Edition { get; }
        IEnumerable<string> RequiredFeatures { get; }
    }
}
