using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AzureOpenAI.Models.Requests.Analysis;

public class TokenizeTextRequest
{
    public string Text { get; set; }
    
    [StaticDataSource(typeof(EncodingDataSourceHandler))]
    public string? Encoding { get; set; }
}