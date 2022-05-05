using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.VSIntegration.UI;
using VS_RustAnalyzer.Cargo;

namespace VS_RustAnalyzer
{
    [Export(typeof(INodeExtender))]
    internal class RustWorkspaceNodeExtender : INodeExtender
    {


        public IChildrenSource ProvideChildren(WorkspaceVisualNodeBase parentNode)
        {
            if(parentNode is IFileNode && Util.IsCargoFile(Path.GetFileName((parentNode as IFileNode).FullPath)))
                return new CargoChildrenSource(this, parentNode);
            return null;
        }

        public IWorkspaceCommandHandler ProvideCommandHandler(WorkspaceVisualNodeBase parentNode)
        {
            return null;
        }

        public class CargoChildrenSource : IChildrenSource
        {
            private INodeExtender _source;
            private WorkspaceVisualNodeBase _parentNode;
            private IWorkspace _workspace;
            private ICargoReaderService _cargoReaderService;

            public CargoChildrenSource(INodeExtender extender, WorkspaceVisualNodeBase parentNode)
            {
                this._source = extender;
                this._parentNode = parentNode;
                this._workspace = parentNode.Workspace;
                this._cargoReaderService = _workspace.GetService<ICargoReaderService>();
            }

            public INodeExtender Extender => _source;

            public int Order => 1;

            public bool ForceExpanded => false;

            public void Dispose()
            {
            }

            public Task<IReadOnlyCollection<WorkspaceVisualNodeBase>> GetCollectionAsync()
            {
                var manifest = _cargoReaderService.CargoManifestForFile((_parentNode as IFileNode).FullPath);
                List<WorkspaceVisualNodeBase> children = new List<WorkspaceVisualNodeBase>();
                foreach(var bin in manifest.BinTargetPaths("dev"))
                {
                    var node = new CargoTargetNode(_parentNode, Path.GetFileName(bin));
                    children.Add(node);
                }
                return Task.FromResult(children as IReadOnlyCollection<WorkspaceVisualNodeBase>);
            }

            class CargoTargetNode : WorkspaceVisualNodeBase
            {
                private string _filePath;
                public CargoTargetNode(WorkspaceVisualNodeBase parent, string path) : base(parent)
                {
                    this._filePath = path;
                    this.NodeMoniker = path;
                }

                protected override void OnInitialized()
                {
                    base.OnInitialized();
                    UINode.Text = _filePath;
                    var executable = KnownMonikers.Console;
                    SetIcon(executable.Guid, executable.Id);
                    /*
                    SetExpandedIcon(executable.Guid, executable.Id);
                    SetOverlayIcon(executable.Guid, executable.Id);
                    SetStateIcon(executable.Guid, executable.Id);
                    */
                }

                public override int Compare(WorkspaceVisualNodeBase right)
                {
                    CargoTargetNode node = right as CargoTargetNode;
                    if (node == null)
                        return -1;
                    return _filePath.CompareTo(node._filePath);
                }
            }
        }
    }
}
