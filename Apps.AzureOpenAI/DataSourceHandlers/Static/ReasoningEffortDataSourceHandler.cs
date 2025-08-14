using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AzureOpenAI.DataSourceHandlers.Static;

public class ReasoningEffortDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return
        [
            new("low", "Low"),
            new("medium", "Medium"),
            new("high", "High")
        ];
    }
}