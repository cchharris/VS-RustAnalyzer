using Microsoft.VisualStudio.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer.Cargo
{
    [ExportWorkspaceServiceFactory(WorkspaceServiceFactoryOptions.None, typeof(ICargoReaderService))]
    internal class CargoReaderServiceFactory : IWorkspaceServiceFactory
    {
        public object CreateService(IWorkspace workspaceContext)
        {
            return new CargoReaderService(workspaceContext);
        }
    }

    internal class CargoReaderService : ICargoReaderService
    {
        private readonly IWorkspace _workspace;
        public CargoReaderService(IWorkspace workspace)
        {
            this._workspace = workspace;
        }

        public ICargoManifest CargoManifestForFile(string path)
        {
            // Todo: Cache
            return new CargoManifest(path);
        }
    }
}
