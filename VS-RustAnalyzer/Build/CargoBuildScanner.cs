using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VS_RustAnalyzer.Cargo;

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

                    ret.Add(new FileDataValue(new Guid(Builds.BuildType), "Build", null, context: Builds.BuildContextInstance.BuildConfiguration));
                    IPropertySettings launchSettings = new PropertySettings
                    {
                        [LaunchConfigurationConstants.NameKey] = cargoManifest.PackageName,
                        [LaunchConfigurationConstants.DebugTypeKey] = LaunchConfigurationConstants.NativeOptionKey,
                        [LaunchConfigurationConstants.ArgsKey] = "echo foo",
                    };

                    ret.Add(new FileDataValue(
                        DebugLaunchActionContext.ContextTypeGuid,
                        DebugLaunchActionContext.IsDefaultStartupProjectEntry,
                        launchSettings));
                }

                return await Task.FromResult((T)(IReadOnlyCollection<FileDataValue>)ret);
            }
        }
    }
}
