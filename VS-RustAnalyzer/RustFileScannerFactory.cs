using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Indexing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VS_RustAnalyzer
{
    [ExportFileScanner(ProviderType,
        "RustFileScanner",
        new string[] {
            Util.RustFileExtension,
            Util.CargoFileExtension
        },
        new Type[] { typeof(IReadOnlyCollection<FileDataValue>) })]
    internal class RustFileScannerFactory : IWorkspaceProviderFactory<IFileScanner>
    {
        public const string ProviderType = "D81EDC58-9B86-4EFB-B4B9-6985515A4CF4";

        public IFileScanner CreateProvider(IWorkspace workspaceContext)
        {
            return new FileScanner(workspaceContext);
        }
        internal class FileScanner : IFileScanner
        {
            private readonly IWorkspace _workspace;
            internal FileScanner(IWorkspace workspace)
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

                if (Util.IsRustFile(filePath))
                {
                    ret.Add(new FileDataValue(new Guid(RustFileContextFactory.ProviderType), Path.GetFileName(filePath), filePath, context: null));
                }
                else if (Util.IsCargoFile(filePath))
                {
                    ret.Add(new FileDataValue(new Guid(RustFileContextFactory.ProviderType), Path.GetFileName(filePath), filePath, context: null));
                    ret.Add(new FileDataValue(new Guid(Builds.BuildType), "Build", null, context: Builds.BuildContextInstance.BuildConfiguration));
                }

                return await Task.FromResult((T)(IReadOnlyCollection<FileDataValue>)ret);
            }
        }
    }
}
