using Apps.AzureOpenAI.Extensions;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AzureOpenAI.DataSourceHandlers;

public class PenaltyDataSourceHandler : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData() => DataSourceHandlersExtensions
        .GenerateFormattedFloatArray(-2.0f, 2.0f, 0.1f)
        .ToDictionary(p => p, p => p);
}