using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Indexing;
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
    [ExportFileScanner(ProviderType,
        "Rust",
        new string[] {
            Util.CargoFileExtension
        },
        new Type[] { typeof(IReadOnlyCollection<FileDataValue>), typeof(IReadOnlyCollection<FileReferenceInfo>) })]
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

                if (typeof(T) == FileScannerTypeConstants.FileDataValuesType)
                {
                    var ret = new List<FileDataValue>();

                    if (Util.IsCargoFile(filePath))
                    {
                        var readerService = _workspace.GetService<ICargoReaderService>();
                        var cargoManifest = readerService.CargoManifestForFile(filePath);

                        //TODO Limit only to bin
                        //TODO Get output location
                        // See: https://doc.rust-lang.org/cargo/guide/build-cache.html
                        var output = Path.Combine(Path.GetDirectoryName(filePath), "target", "debug", cargoManifest.PackageName) + ".exe";

                        foreach (var profile in cargoManifest.Profiles)
                        {
                            ret.Add(new FileDataValue(BuildConfigurationContext.ContextTypeGuid,
                                BuildConfigurationContext.DataValueName,
                                value: null,
                                target: null,
                                context: profile));
                            foreach (var binTarget in cargoManifest.BinTargetPaths(profile))
                            {
                                string name = $"{Path.GetFileName(binTarget)} [{cargoManifest.PackageName}]";
                                IPropertySettings launchSettings = new PropertySettings
                                {
                                    [LaunchConfigurationConstants.NameKey] = name,
                                    [LaunchConfigurationConstants.DebugTypeKey] = LaunchConfigurationConstants.NativeOptionKey,
                                    [LaunchConfigurationConstants.ProgramKey] = binTarget,
                                };

                                ret.Add(new FileDataValue(
                                    DebugLaunchActionContext.ContextTypeGuid,
                                    DebugLaunchActionContext.IsDefaultStartupProjectEntry,
                                    launchSettings));
                            }
                        }
                    }

                    return await Task.FromResult((T)(IReadOnlyCollection<FileDataValue>)ret);
                }
                else if  (typeof(T) == FileScannerTypeConstants.FileReferenceInfoType)
                {
                    var ret = new List<FileReferenceInfo>();

                    if (Util.IsCargoFile(filePath))
                    {
                        var readerService = _workspace.GetService<ICargoReaderService>();
                        var cargoManifest = readerService.CargoManifestForFile(filePath);

                        foreach (var profile in cargoManifest.Profiles)
                        {
                            foreach (var target in cargoManifest.BinTargetPaths(profile))
                            {
                                ret.Add(new FileReferenceInfo(target, null, profile, (int)FileReferenceInfoType.Output));
                            }
                        }
                    }

                    return await Task.FromResult((T)(IReadOnlyCollection<FileReferenceInfo>)ret);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
