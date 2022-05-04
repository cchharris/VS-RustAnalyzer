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
        Binary, // bin
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
        string CratePath { get; }
        TargetForProfileDelegate TargetPath { get; }
        TargetType TargetType { get; }
        CrateType CrateType { get; }
    }

    public class CargoTarget : ICargoTarget
    {
        private string _name;
        private string _srcPath;
        private string _cratePath;
        private TargetForProfileDelegate _targetPath;
        TargetType _targetType;
        CrateType _crateType;

        public CargoTarget(string name, string srcPath, string cratePath, TargetForProfileDelegate targetPath, TargetType targetType, CrateType crateType)
        {
            _name = name;
            _srcPath = srcPath;
            _cratePath = cratePath;
            _targetPath = targetPath;
            _targetType = targetType;
            _crateType = crateType;
        }

        public string Name => _name;

        public string SrcPath => _srcPath;
        public string CratePath => _cratePath;

        public TargetForProfileDelegate TargetPath => _targetPath;

        public TargetType TargetType => _targetType;

        public CrateType CrateType => _crateType;
    }
}
