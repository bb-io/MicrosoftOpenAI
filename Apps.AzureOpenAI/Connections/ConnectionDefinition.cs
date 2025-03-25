using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.AzureOpenAI.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups =>
    [
        new()
        {
            Name = "Developer API token",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>
            {
                new("url") { DisplayName = "Resource URL" },
                new("deployment") { DisplayName = "Deployment name" },
                new("apiKey") { DisplayName = "API key", Sensitive = true }
            }
        }
    ];

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        return values.Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value));
    }
}