using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AzureOpenAI.Models.Requests.Batch;

public class BatchIdentifier
{
    [Display("Batch ID"), DataSource(typeof(BatchDataSourceHandler))]
    public string BatchId { get; set; } = default!;
}