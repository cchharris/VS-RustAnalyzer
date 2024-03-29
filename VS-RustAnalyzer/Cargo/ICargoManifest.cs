﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VS_RustAnalyzer.Cargo.Toml;

namespace VS_RustAnalyzer.Cargo
{
    public interface ICargoManifest
    {
        string PackageName { get; }

        List<ICargoTarget> Targets { get; }

        IEnumerable<string> Profiles { get; }

        IEnumerable<string> BinTargetPaths(string profile);

        IEnumerable<ICargoTarget> EnumerateTargetsByType(TargetType type);
    }
}
