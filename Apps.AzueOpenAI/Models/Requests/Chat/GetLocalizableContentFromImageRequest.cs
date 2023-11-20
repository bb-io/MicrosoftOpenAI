using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class GetLocalizableContentFromImageRequest : BaseChatRequest
{
    public File Image { get; set; }
}