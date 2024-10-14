using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AzureOpenAI.Models.Requests;

public class TextChatModelIdentifier
{
    [Display("Model ID")]
    [DataSource(typeof(TextChatModelDataSourceHandler))]
    public string ModelId { get; set; } = default!;
}