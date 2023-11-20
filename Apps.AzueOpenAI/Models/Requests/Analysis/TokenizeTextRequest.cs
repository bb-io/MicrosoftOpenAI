using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AzureOpenAI.Models.Requests.Analysis;

public class TokenizeTextRequest
{
    public string Text { get; set; }
    
    [DataSource(typeof(EncodingDataSourceHandler))]
    public string? Encoding { get; set; }
}