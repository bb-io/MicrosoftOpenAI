using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class ChatWithImageRequest : ChatRequest
{
    public FileReference Image { get; set; }
}