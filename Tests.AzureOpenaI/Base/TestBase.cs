using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Extensions.Configuration;

namespace Tests.AzureOpenAI.Base;

public class TestBase
{
    public IEnumerable<AuthenticationCredentialsProvider> Creds { get; private set; }
    public InvocationContext InvocationContext { get; private set; }
    public FileManager FileManager { get; private set; }

    public TestBase()
    {
        InitializeConfiguration();
    }

    private void InitializeConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        Creds = LoadCredentialsFromConfig(config);
        
        InvocationContext = new InvocationContext
        {
            AuthenticationCredentialsProviders = Creds
        };
        
        var folderLocation = config.GetSection("TestFolder").Value 
            ?? throw new InvalidOperationException("TestFolder configuration is missing");
        
        FileManager = new FileManager(folderLocation);
    }

    private static IEnumerable<AuthenticationCredentialsProvider> LoadCredentialsFromConfig(IConfiguration config)
    {
        return config.GetSection("ConnectionDefinition")
            .GetChildren()
            .Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value!))
            .ToList();
    }
}
