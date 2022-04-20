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
            BuildContextTypes.BuildContextType
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
                    var launchCommand = new LaunchCommand("cargo", "build", LaunchCommandOption.None, workingDirectory: Path.GetDirectoryName(filePath));
                    var buildActionContext = new BuildActionContext(new LaunchCommand[] { launchCommand }, "Cargo build configuration");
                    fileContexts.Add(new FileContext(new Guid(ProviderType),
                        BuildActionContext.ContextTypeGuid, buildActionContext, new string[] {filePath}));
                }

                return await Task.FromResult(fileContexts);
            }
        }
    }
}
