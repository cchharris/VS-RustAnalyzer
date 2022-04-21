using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft;

namespace VS_RustAnalyzer
{
    internal class GeneralOptions : DialogPage
    {
        private GeneralSettings _settings;
        public GeneralOptions()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var componentModel = ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            Assumes.Present(componentModel);
            _settings = componentModel.GetService<GeneralSettings>();
        }

        [Category("Setup")]
        [DisplayName("Path to rust-analyzer.exe")]
        [Description("If null/empty, searches PATH for rust-analyzer.exe.  Set a full path to a rust-analyzer.exe to override.")]
        public string RustAnalyzerPath
        {
            get { return _settings.RustAnalyzerPath; }
            set { _settings.RustAnalyzerPath = value; }
        }

        public override void LoadSettingsFromStorage()
        {
            _settings.Load();
        }

        public override void SaveSettingsToStorage()
        {
            _settings.Save();
        }
    }
}
