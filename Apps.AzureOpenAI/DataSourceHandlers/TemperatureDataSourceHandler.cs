using Apps.AzureOpenAI.Extensions;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AzureOpenAI.DataSourceHandlers;

public class TemperatureDataSourceHandler : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData() => DataSourceHandlersExtensions
        .GenerateFormattedFloatArray(0.0f, 2.0f, 0.1f)
        .ToDictionary(t => t, t => t);
}