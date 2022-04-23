using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using Microsoft.VisualStudio.Workspace.Extensions.Build;
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
    [ExportFileContextProvider(ProviderType,
        new string[] {
            PackageIds.CargoFileContextType,
            BuildContextTypes.BuildContextType,
            BuildContextTypes.CleanContextType,
            BuildConfigurationContext.ContextType,
        })]
    internal class CargoContextProviderFactory : IWorkspaceProviderFactory<IFileContextProvider>
    {
        public const string ProviderType = "D2EFBEFC-0C3E-43B4-B4A2-D3A210C4E6D6";
        public static readonly Guid ProviderTypeGuid = new Guid(ProviderType);

        public IFileContextProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new CargoContextProvider(workspaceContext);
        }

        internal class CargoContextProvider : IFileContextProvider
        {
            private readonly IWorkspace _workspace;
            private readonly ICargoReaderService _cargoReaderService;
            internal CargoContextProvider(IWorkspace workspace)
            {
                this._workspace = workspace;
                _cargoReaderService = _workspace.GetService<ICargoReaderService>();
            }

            public async Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, CancellationToken cancellationToken)
            {
                var fileContexts = new List<FileContext>();
                if (Util.IsCargoFile(filePath))
                {
                    var cargoManifest = _cargoReaderService.CargoManifestForFile(filePath);
                    /**
                     * These are the menu items across the top of Visual Studio
                     */
                    foreach (var profile in cargoManifest.Profiles)
                    {
                        var buildLaunchCommand = new LaunchCommand("cargo", $"build --profile {profile}",
                            LaunchCommandOption.None,
                            workingDirectory: Path.GetDirectoryName(filePath),
                            projectFullPath:filePath);
                        var buildActionContext = new BuildActionContext(new LaunchCommand[] { buildLaunchCommand }, profile);
                        fileContexts.Add(new FileContext(ProviderTypeGuid,
                            BuildActionContext.ContextTypeGuid, buildActionContext, new string[] { filePath }));
                        //fileContexts.Add(new FileContext(ProviderTypeGuid, BuildConfigurationContext.ContextTypeGuid, new CargoBuildContext(profile), new string[] { filePath }));
                    }
                    var cleanLaunchCommand = new LaunchCommand("cargo", "clean",
                        LaunchCommandOption.None,
                        workingDirectory: Path.GetDirectoryName(filePath),
                        projectFullPath:filePath);
                    var cleanActionContext = new BuildActionContext(new LaunchCommand[] { cleanLaunchCommand }, "Cargo clean configuration");
                    fileContexts.Add(new FileContext(ProviderTypeGuid,
                        BuildActionContext.CleanContextTypeGuid, cleanActionContext, new string[] { filePath }));
                }

                return await Task.FromResult(fileContexts);
            }
        }
    }
}
