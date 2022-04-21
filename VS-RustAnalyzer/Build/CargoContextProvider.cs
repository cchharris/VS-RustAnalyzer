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

namespace VS_RustAnalyzer.Build
{
    [ExportFileContextProvider(ProviderType,
        new string[] {
            PackageIds.CargoFileContextType,
            BuildContextTypes.BuildContextType,
            BuildContextTypes.CleanContextType,
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
            internal CargoContextProvider(IWorkspace workspace)
            {
                this._workspace = workspace;
            }

            public async Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, CancellationToken cancellationToken)
            {
                var fileContexts = new List<FileContext>();
                if (Util.IsCargoFile(filePath))
                {
                    /**
                     * These are the menu items across the top of Visual Studio
                     */
                    var buildLaunchCommand = new LaunchCommand("cargo", "build", LaunchCommandOption.None, workingDirectory: Path.GetDirectoryName(filePath));
                    var buildActionContext = new BuildActionContext(new LaunchCommand[] { buildLaunchCommand }, "Cargo build configuration");
                    fileContexts.Add(new FileContext(new Guid(ProviderType),
                        BuildActionContext.ContextTypeGuid, buildActionContext, new string[] {filePath}));
                    var cleanLaunchCommand = new LaunchCommand("cargo", "clean", LaunchCommandOption.None, workingDirectory: Path.GetDirectoryName(filePath));
                    var cleanActionContext = new BuildActionContext(new LaunchCommand[] { cleanLaunchCommand }, "Cargo clean configuration");
                    fileContexts.Add(new FileContext(new Guid(ProviderType),
                        BuildActionContext.CleanContextTypeGuid, cleanActionContext, new string[] {filePath}));
                }

                return await Task.FromResult(fileContexts);
            }
        }
    }
}
