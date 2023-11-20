using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class LocalizeTextRequest
{
    public string Text { get; set; }
        
    [DataSource(typeof(LocaleDataSourceHandler))]
    public string Locale { get; set; }
}