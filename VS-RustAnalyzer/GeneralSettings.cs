using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer
{
    [Export]
    internal class GeneralSettings
    {
        private IServiceProvider _serviceProvider;
        private const string _settingCollection = "VS-RustAnalyzer";
        private const string _rustAnalyzerPath = "rust_analyzer_path";

        public string RustAnalyzerPath { get; set; } = "";

        [ImportingConstructor]
        public GeneralSettings([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Load()
        {
            var settingsManager = new ShellSettingsManager(_serviceProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            if (!userSettingsStore.CollectionExists(_settingCollection))
                userSettingsStore.CreateCollection(_settingCollection);

            if (userSettingsStore.PropertyExists(_settingCollection, _rustAnalyzerPath))
                RustAnalyzerPath = userSettingsStore.GetString(_settingCollection, _rustAnalyzerPath);
        }

        public void Save()
        {
            var settingsManager = new ShellSettingsManager(_serviceProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            if (!userSettingsStore.CollectionExists(_settingCollection))
                userSettingsStore.CreateCollection(_settingCollection);

            userSettingsStore.SetString(_settingCollection, _rustAnalyzerPath, RustAnalyzerPath);
        }
    }
}
