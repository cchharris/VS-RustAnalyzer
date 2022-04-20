using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using Microsoft.VisualStudio.Workspace.Extensions.VS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VS_RustAnalyzer.Cargo.Json;

namespace VS_RustAnalyzer
{
    [ExportFileContextActionProvider(ProviderType, new string[] { PackageIds.CargoFileContextType })]
    internal class CargoFileContextActionProviderFactory : IWorkspaceProviderFactory<IFileContextActionProvider>
    {
        public const string ProviderType = "2CC51F9D-9749-4CFD-A8E9-B6E9FE2E787B";
        public static readonly Guid ProviderTypeGuid = new Guid(ProviderType);

        public IFileContextActionProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new FileContextActionProvider(workspaceContext);
        }

        internal class FileContextActionProvider : IFileContextActionProvider
        {
            private readonly IWorkspace _workspace;
            internal FileContextActionProvider(IWorkspace workspace)
            {
                _workspace = workspace;
            }

            public async Task<IReadOnlyList<IFileContextAction>> GetActionsAsync(string filePath, FileContext fileContext, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<IFileContextAction> actions = new List<IFileContextAction>();
                actions.Add(new BuildContextAction(_workspace, filePath, fileContext));
                return await Task.FromResult(actions);
            }

            /// <summary>
            ///  Taken from https://docs.microsoft.com/en-us/visualstudio/extensibility/workspace-build?view=vs-2022
            /// </summary>
            internal enum BuildCommandId
            {
                Build = 0x1000,
                Rebuild = 0x1010,
                Clean = 0x1020
            }

            internal class BuildContextAction : IFileContextAction, IVsCommandItem
            {
                private readonly IWorkspace _workspace;
                private readonly string _filePath;
                private readonly FileContext _context;
                internal BuildContextAction(IWorkspace workspace, string filepath, FileContext context)
                {
                    this._workspace = workspace;
                    this._filePath = filepath;
                    this._context = context;
                }

                public FileContext Source => _context;

                public string DisplayName => "Build";

                /// <summary>
                ///  Taken from https://docs.microsoft.com/en-us/visualstudio/extensibility/workspace-build?view=vs-2022
                /// </summary>
                public Guid CommandGroup => new Guid("16537f6e-cb14-44da-b087-d1387ce3bf57");

                public uint CommandId => (uint)BuildCommandId.Build;

                internal class BuildResult : IFileContextActionResult
                {
                    bool _success;
                    internal BuildResult(bool success)
                    {
                        this._success = success;
                    }
                    public bool IsSuccess => _success;
                }

                public async Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await TaskScheduler.Default;
                    return await _workspace.JTF.RunAsync<IFileContextActionResult>(async () => {
                        ProcessStartInfo info = new ProcessStartInfo()
                        {
                            FileName = "cargo", 
                            Arguments = "build --message-format json",
                            WorkingDirectory = Path.GetDirectoryName(_filePath),
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                        };
                        var result = Process.Start(info);

                        result.OutputDataReceived += (a, e) => {
                            if (string.IsNullOrEmpty(e.Data)) return;

                            var data = JsonConvert.DeserializeObject<CompilerMessage>(e.Data);
                            if (data == null) return;
                            if (!data.Reason.Equals("compiler-message")) return;

                            List<BuildMessage> messages = new List<BuildMessage>();
                            var messagedata = data.MessageData;
                            string errorLevel = messagedata.Level;
                            BuildMessage.TaskType taskType;
                            if (errorLevel.Equals("error", StringComparison.OrdinalIgnoreCase))
                            {
                                taskType = BuildMessage.TaskType.Error;
                            }
                            else if (errorLevel.Equals("warning", StringComparison.OrdinalIgnoreCase))
                            {
                                taskType = BuildMessage.TaskType.Warning;
                            }
                            else
                            {
                                taskType = BuildMessage.TaskType.None;
                            }
                            BuildMessage message = new BuildMessage();
                            message.ProjectFile = data.ManifestPath;
                            message.Type =  taskType;
                            message.LogMessage = data.MessageData.Rendered;
                            _workspace.GetBuildMessageService().ReportBuildMessages(messages);
                        };

                        result.BeginErrorReadLine();
                        result.BeginOutputReadLine();
                        result.WaitForExit();
                        return await Task.FromResult(new BuildResult(result.ExitCode == 0));
                    }).JoinAsync();
                }
            }
        }
    }
}
