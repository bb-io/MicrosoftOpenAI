using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class GetLocalizableContentFromImageRequest : BaseChatRequest
{
    public FileReference Image { get; set; }
}