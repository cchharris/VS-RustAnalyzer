using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Indexing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VS_RustAnalyzer.Cargo;
using static VS_RustAnalyzer.Builds;

namespace VS_RustAnalyzer.Build
{
    [ExportFileScanner(ProviderType,
        "Rust",
        new string[] {
            Util.CargoFileExtension
        },
        new Type[] { typeof(IReadOnlyCollection<FileDataValue>) })]
    internal class CargoBuildScanner : IWorkspaceProviderFactory<IFileScanner>
    {
        public const string ProviderType = "F5628EAD-8A24-4683-B597-D8314B971ED6";
        public static readonly Guid ProviderTypeGuid = new Guid(ProviderType);

        public IFileScanner CreateProvider(IWorkspace workspaceContext)
        {
            return new CargoBuildScannerImpl(workspaceContext);
        }

        internal class CargoBuildScannerImpl : IFileScanner
        {
            private readonly IWorkspace _workspace;
            public CargoBuildScannerImpl(IWorkspace workspace)
            {
                _workspace = workspace;
            }

            public async Task<T> ScanContentAsync<T>(string filePath, CancellationToken cancellationToken) where T : class
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (typeof(T) != FileScannerTypeConstants.FileDataValuesType)
                {
                    throw new NotImplementedException();
                }

                var ret = new List<FileDataValue>();

                if (Util.IsCargoFile(filePath))
                {
                    var readerService = _workspace.GetService<ICargoReaderService>();
                    var cargoManifest = readerService.CargoManifestForFile(filePath);

                    //TODO Limit only to bin
                    //TODO Get output location
                    // See: https://doc.rust-lang.org/cargo/guide/build-cache.html
                    var output = Path.Combine(Path.GetDirectoryName(_workspace.MakeRelative(filePath)), "target", "debug", cargoManifest.PackageName) + ".exe";

                    foreach (var profile in cargoManifest.Profiles)
                    {
                        ret.Add(new FileDataValue(BuildConfigurationContext.ContextTypeGuid, BuildConfigurationContext.DataValueName, value: null, target: null, context: profile));
                    }

                    foreach (var binTarget in cargoManifest.BinTargets)
                    {
                        IPropertySettings launchSettings = new PropertySettings
                        {
                            //["StartupProject"] = filePath,
                            [LaunchConfigurationConstants.NameKey] = $"{binTarget}.exe [{cargoManifest.PackageName}]",
                            [LaunchConfigurationConstants.DebugTypeKey] = LaunchConfigurationConstants.NativeOptionKey,
                            [LaunchConfigurationConstants.ProjectKey] = output,
                            [LaunchConfigurationConstants.ProjectTargetKey] = $"",
                        };

                        ret.Add(new FileDataValue(
                            DebugLaunchActionContext.ContextTypeGuid,
                            DebugLaunchActionContext.IsDefaultStartupProjectEntry,
                            launchSettings));
                    }
                }

                return await Task.FromResult((T)(IReadOnlyCollection<FileDataValue>)ret);
            }
        }
    }
}
