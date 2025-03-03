using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AzureOpenAI.Models.Requests.Image;

public class ImageRequest
{
    public string Prompt { get; set; }
        
    [StaticDataSource(typeof(ImageSizeDataSourceHandler))]
    public string? Size { get; set; }
}