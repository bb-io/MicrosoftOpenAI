using Apps.AzureOpenAI.Actions;
using Apps.AzureOpenAI.Models.Requests.Chat;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AzureOpenAI.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        try
        {            
            var actions = new ChatActions(new InvocationContext() { AuthenticationCredentialsProviders = authProviders }, null!);
            var userPrompt = "Hello, how are you?";
            var systemPrompt = "You are a helpful assistant.";

            var result = await actions.ExecuteOpenAIRequestAsync(new Models.Entities.ExecuteOpenAIRequestParameters(userPrompt, systemPrompt, "2024-08-01-preview", new ChatRequest(), null));
            return new()
            {
                IsValid = !string.IsNullOrEmpty(result.result),
                Message = result.result,
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
