using Apps.AzureOpenAI.Actions;
using Apps.AzureOpenAI.Models.Requests.Chat;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Apps.AzureOpenAI.Connections
{
    public class ConnectionValidator : IConnectionValidator
    {
        public async ValueTask<ConnectionValidationResponse> ValidateConnection(
            IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
        {
            var actions = new ChatActions(new InvocationContext() { AuthenticationCredentialsProviders = authProviders });
            try
            {
                //await actions.ChatMessageRequest(new ChatRequest() { Message = "hello" });
                return new()
                {
                    IsValid = true
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    IsValid = false,
                    Message = ex.Message
                };
            }
        }
    }
}
