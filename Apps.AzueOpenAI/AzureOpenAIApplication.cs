using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.AzureOpenAI;

public class AzureOpenAIApplication : IApplication, ICategoryProvider
{
    public IEnumerable<ApplicationCategory> Categories
    {
        get => [ApplicationCategory.ArtificialIntelligence, ApplicationCategory.AzureApps];
        set { }
    }
    
    public string Name
    {
        get => "Azure OpenAI";
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}