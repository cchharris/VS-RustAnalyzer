using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace VS_RustAnalyzer
{
    internal class GeneralOptions : DialogPage
    {
        private string rustAnalyzerPath;

        [Category("Setup")]
        [DisplayName("Path to rust-analyzer.exe")]
        [Description("Full path to the rust-analyzer exe")]
        public string RustAnalyzerPath {
            get { return rustAnalyzerPath; }
            set { rustAnalyzerPath = value; }
        }
    }
}
