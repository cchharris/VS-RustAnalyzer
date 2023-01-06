using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using Microsoft.VisualStudio.Workspace.Debug;
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
            DebugLaunchActionContext.ContextType,
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
                var cargoFilePath = Util.GetCargoFileForPath(filePath, _workspace.Location);
                string cargoFolder = Path.GetDirectoryName(cargoFilePath);

                var cargoManifest = _cargoReaderService.CargoManifestForFile(cargoFilePath);
                /**
                 * These are the menu items across the top of Visual Studio
                 */
                foreach (var profile in cargoManifest.Profiles)
                {
                    var buildLaunchCommand = new LaunchCommand("cargo", $"build --profile {profile} --message-format short",
                        LaunchCommandOption.None,
                        workingDirectory: cargoFolder,
                        projectFullPath: cargoFilePath);
                    var buildActionContext = new BuildActionContext(new LaunchCommand[] { buildLaunchCommand }, profile);
                    fileContexts.Add(new FileContext(ProviderTypeGuid,
                        BuildActionContext.ContextTypeGuid, buildActionContext, new string[] { cargoFilePath }));

                    var cleanLaunchCommand = new LaunchCommand("cargo", $"clean --profile {profile}",
                        LaunchCommandOption.None,
                        workingDirectory: cargoFolder,
                        projectFullPath: cargoFilePath);
                    var cleanActionContext = new BuildActionContext(new LaunchCommand[] { cleanLaunchCommand }, profile);
                    fileContexts.Add(new FileContext(ProviderTypeGuid,
                        BuildActionContext.CleanContextTypeGuid, cleanActionContext, new string[] { cargoFilePath }));
                }

                return await Task.FromResult(fileContexts);
            }
        }
    }
}
