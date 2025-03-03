using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using RestSharp;

namespace Apps.AzureOpenAI.Api;

public class AzureOpenAIRequest(
    string resource,
    Method method,
    IEnumerable<AuthenticationCredentialsProvider> creds)
    : BlackBirdRestRequest(resource, method, creds)
{
    protected override void AddAuth(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        this.AddHeader("api-key", creds
            .First(x => x.KeyName == "apiKey").Value);
    }
}