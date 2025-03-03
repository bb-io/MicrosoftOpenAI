using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AzureOpenAI.DataSourceHandlers;

public class EncodingDataSourceHandler : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData() => new()
    {
        ["cl100k_base"] = "cl100k_base",
        ["p50k_base"] = "p50k_base",
    };
}