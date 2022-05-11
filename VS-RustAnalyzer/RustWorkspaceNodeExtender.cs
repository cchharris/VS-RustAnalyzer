using System;
using EnvDTE;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.VSIntegration.UI;
using VS_RustAnalyzer.Cargo;
using Microsoft.VisualStudio.Imaging.Interop;

namespace VS_RustAnalyzer
{
    // https://doc.rust-lang.org/book/ch14-03-cargo-workspaces.html
    /* I'm still figuring out how I want these laid out.
     *  Old C# has
     *  
     *  Solution 'Name' (2 of 2 projects)
     *  |-Proj Name
     *  | |-Properties
     *  | | |-AssemblyInfo.cs
     *  | |-References
     *  | | |-Analyzers
     *  | | | |-Analyser
     *  | | |-PackageName
     *  | | |-NugetName
     *  | |-Folders
     *  | |-File.cs
     *  | |-Code.cs
     * 
     * .NET Core changed this to
     * 
     *  Solution 'Name' (2 of 2 projects) # Can unload and reload subsets
     *  |-Proj Name
     *  | |-Dependencies
     *  | | |-Analyzers
     *  | | | |-Analyser
     *  | | |-Frameworks
     *  | | | |-Microsoft.NETCore.App
     *  | | | | |-Microsoft.CSharp
     *  | | | | |-Microsoft.*
     *  | | |-Packages
     *  | | | |-NugetName (version#)
     *  | | | | |-Nuget Dependencies (version#)
     *  | | |-Projects
     *  | | | |-ProjName
     *  | |-Folders
     *  | |-File.cs
     *  | |-Code.cs
     *  
     *  What we probably want is
     *  
     *  Workspace 'Name' (2 of 2 Packages) # Allow unloading and reloading subsets? Similar to C#
     *  |-Cargo.toml
     *  |-Config.toml
     *  |-Cargo.lock
     *  |-Package Name        # This is root if we don't see any workspace and we start in a folder containing a package Cargo.toml
     *  | |-Cargo.toml
     *  | |-Config.toml
     *  | |-Cargo.lock          # If not a workspace
     *  | |-Crates
     *  | | |-Lib
     *  | | |-PackageBin
     *  | | |-InferredBin
     *  | | |-ExplicitBin
     *  | | |-Unit Tests
     *  | | |-Integration Tests
     *  | | |-Explicit Tests
     *  | | |-Inferred Examples
     *  | | |-Explicit Examples
     *  | | |-Unit Bench
     *  | | |-Inferred Bench
     *  | | |-Explicit Bench
     *  | |-Dependencies
     *  | | |-Rust Toolchain
     *  | | | |-Version, nightly, etc.
     *  | | |-Crates
     *  | | | |-Crate name (version)
     *  | | | | |-Features
     *  | | | | | |-Required feature
     *  | | | | |-Dependency (version)
     *  | |-Folders
     *  | |-File.rs
     *  | |-main.rs
     */
    [Export(typeof(INodeExtender))]
    internal class RustWorkspaceNodeExtender : INodeExtender
    {
        protected SVsServiceProvider _provider;

        [ImportingConstructor]
        public RustWorkspaceNodeExtender([Import] SVsServiceProvider serviceProvider)
        {
            _provider = serviceProvider;
        }

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
                //TODO: Make independent node?
                _parentNode.SetIcon(Assets.Guid, Assets.RSPackageClosedId);
                _parentNode.SetExpandedIcon(Assets.Guid, Assets.RSPackageOpenedId);
                var manifestPath = (_parentNode as IFileNode).FullPath;
                var manifest = _cargoReaderService.CargoManifestForFile(manifestPath);
                List<WorkspaceVisualNodeBase> children = new List<WorkspaceVisualNodeBase>();
                foreach(var bin in manifest.EnumerateTargetsByType(TargetType.Library))
                {
                    foreach (var targetPath in bin.TargetPath(CargoManifest.ProfileDev))
                    {
                        var node = new CargoTargetNode((Extender as RustWorkspaceNodeExtender)._provider, _parentNode, Path.GetFileName(targetPath), Path.Combine(Path.GetDirectoryName(manifestPath), targetPath), Assets.RSClassLibrarymoniker);
                        children.Add(node);
                    }
                }
                foreach(var bin in manifest.EnumerateTargetsByType(TargetType.Binary))
                {
                    foreach (var targetPath in bin.TargetPath(CargoManifest.ProfileDev))
                    {
                        var node = new CargoTargetNode((Extender as RustWorkspaceNodeExtender)._provider, _parentNode, Path.GetFileName(targetPath), Path.Combine(Path.GetDirectoryName(manifestPath), targetPath), Assets.RSConsoleMoniker);
                        children.Add(node);
                    }
                }
                return Task.FromResult(children as IReadOnlyCollection<WorkspaceVisualNodeBase>);
            }

            class CargoTargetNode : WorkspaceVisualNodeBase, IFileNode
            {
                private readonly SVsServiceProvider _provider;
                private string _fileName;
                private string _filePath;
                private ImageMoniker _moniker;

                public string FileName => _fileName;

                public string FullPath => _filePath;

                public CargoTargetNode(SVsServiceProvider provider, WorkspaceVisualNodeBase parent, string fileName, string filePath, ImageMoniker moniker) : base(parent)
                {
                    this._filePath = filePath;
                    this._provider=provider;
                    this._fileName = fileName;
                    this.NodeMoniker = fileName;
                    this._moniker=moniker;
                }

                public override Func<IEnumerable<WorkspaceVisualNodeBase>, bool, string, bool> InvokeAction {
                    get {
                        // Message = "Mouse", "Keyboard"
                        return delegate (IEnumerable<WorkspaceVisualNodeBase> children, bool unknown, string message)
                        {
                            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                            DTE dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
                            dte.Documents.Open((this.Parent as IFileNode).FullPath);
                            return true;
                        };
                    }
                }

                protected override void OnInitialized()
                {
                    base.OnInitialized();
                    UINode.Text = _fileName;
                    SetIcon(_moniker.Guid, _moniker.Id);
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
                    return _fileName.CompareTo(node._fileName);
                }
            }
        }
    }
}
