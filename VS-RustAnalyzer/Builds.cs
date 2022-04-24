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

        internal class CargoBuildContext : IBuildConfigurationContext
        {
            private string _profile;

            public CargoBuildContext(string profile)
            {
                this._profile = profile;
            }
            public string BuildConfiguration => _profile;
        }
    }
}
