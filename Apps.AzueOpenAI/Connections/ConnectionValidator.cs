using Apps.AzureOpenAI.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;

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
