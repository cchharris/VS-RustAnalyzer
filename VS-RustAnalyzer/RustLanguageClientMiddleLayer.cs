using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Newtonsoft.Json.Linq;

namespace VS_RustAnalyzer
{
    internal class RustLanguageClientMiddleLayer : ILanguageClientMiddleLayer
    {
        public static RustLanguageClientMiddleLayer Instance = new RustLanguageClientMiddleLayer();

        public bool CanHandle(string methodName)
        {
            if (methodName == "textDocument/documentColor" || methodName.StartsWith("textDocument/semanticTokens"))
                return true;
            return false;
        }

        public Task HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification)
        {
            return sendNotification(methodName);
        }

        public Task<JToken> HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
        {
            return sendRequest(methodName);
        }
    }
}
