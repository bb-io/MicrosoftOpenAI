using Azure;
using Azure.AI.OpenAI;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AzureOpenAI.Actions.Base;

public class BaseActions : BaseInvocable
{
    protected readonly OpenAIClient Client;
    protected readonly string DeploymentName;

    protected BaseActions(InvocationContext invocationContext) 
        : base(invocationContext)
    {
        DeploymentName = InvocationContext.AuthenticationCredentialsProviders.First(x => x.KeyName == "deployment")
            .Value;
        Client = new OpenAIClient(
            new Uri(InvocationContext.AuthenticationCredentialsProviders.First(x => x.KeyName == "url").Value),
            new AzureKeyCredential(InvocationContext.AuthenticationCredentialsProviders
                .First(x => x.KeyName == "apiKey").Value));
    }
}