using Microsoft.VisualStudio.Workspace.Build;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer
{
    internal class Builds
    {
        public const string BuildType = "670BD024-805F-4125-9E31-1D0454C7F576";
        public static IBuildConfigurationContext BuildContextInstance = new BuildContext();

        internal class BuildContext : IBuildConfigurationContext
        {
            public string BuildConfiguration => "Cargo Build";
        }
    }
}
