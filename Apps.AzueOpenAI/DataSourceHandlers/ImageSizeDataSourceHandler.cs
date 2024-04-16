using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AzureOpenAI.DataSourceHandlers;

public class ImageSizeDataSourceHandler : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData() => new()
    {
        ["256x256"] = "256x256",
        ["512x512"] = "512x512",
        ["1024x1024"] = "1024x1024",
    };
}