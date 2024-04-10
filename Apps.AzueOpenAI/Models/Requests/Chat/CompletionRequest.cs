namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class CompletionRequest : BaseChatRequest
{
    public string Prompt { get; set; }
}