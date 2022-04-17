using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VS_RustAnalyzer
{
    [ExportFileContextProvider(ProviderType, new string[] { PackageIds.RsFileContextType, PackageIds.CargoFileContextType })]
    internal class RustFileContextFactory : IWorkspaceProviderFactory<IFileContextProvider>
    {
        public const string ProviderType = "23E0D514-FDCE-476F-95F6-74CA640EF7FE";

        public IFileContextProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new FileContextProvider(workspaceContext);
        }

        internal class FileContextProvider : IFileContextProvider
        {
            private readonly IWorkspace _workspace;
            internal FileContextProvider(IWorkspace workspace)
            {
                this._workspace = workspace;
            }

            public async Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, CancellationToken cancellationToken)
            {
                var fileContexts = new List<FileContext>();
                if (Util.IsRustFile(filePath))
                {
                    fileContexts.Add(new FileContext(
                        new Guid(ProviderType),
                        new Guid(PackageIds.RsFileContextType),
                        filePath + '\n', Array.Empty<string>()));
                }
                else if (Util.IsCargoFile(filePath))
                {
                    fileContexts.Add(new FileContext(
                        new Guid(ProviderType),
                        new Guid(PackageIds.CargoFileContextType),
                        filePath + '\n', Array.Empty<string>()));
                }

                return await Task.FromResult(fileContexts.ToArray());
            }
        }
    }
}
