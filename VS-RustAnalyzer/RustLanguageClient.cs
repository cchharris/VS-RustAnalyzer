using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VS_RustAnalyzer
{
    [ContentType("rust")]
    [Export(typeof(ILanguageClient))]
    internal class RustLanguageClient : ILanguageClient, ILanguageClientCustomMessage2
    {
        public string Name => "VS-RustAnalyzer";

        public IEnumerable<string> ConfigurationSections => null;

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => null;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        // TODO: Configure
        public string ExecutablePath => @"rust-analyzer.exe";

        public bool ShowNotificationOnInitializeFailed => true;

        public object MiddleLayer => RustLanguageClientMiddleLayer.Instance;

        public object CustomMessageTarget {get;}

        RustLanguageClient()
        {
            CustomMessageTarget = new MessageHandler();
        }

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            // await Task.Yield(); // Default Boilerplate

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = ExecutablePath;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            Process process = new Process();
            process.StartInfo = info;

            if (process.Start())
            {
                return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
            }

            return null;
        }

        public async Task OnLoadedAsync()
        {
            await StartAsync.InvokeAsync(this, EventArgs.Empty);
        }

        public Task OnServerInitializeFailedAsync(Exception e)
        {
            return Task.CompletedTask;
        }

        public Task OnServerInitializedAsync()
        {
            return Task.CompletedTask;
        }

        public Task<InitializationFailureContext> OnServerInitializeFailedAsync(ILanguageClientInitializationInfo initializationState)
        {
            var ret = new InitializationFailureContext();
            ret.FailureMessage = $"${Name} - {initializationState.StatusMessage} - {initializationState.InitializationException}";
            return Task.FromResult(ret);
        }

        public Task AttachForCustomMessageAsync(JsonRpc rpc)
        {
            return Task.CompletedTask;
        }

        public class MessageHandler
        {

        }
    }
}
