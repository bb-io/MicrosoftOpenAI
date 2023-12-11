namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class ExecuteBlackbirdPromptRequest : BaseChatRequest
{
    public string Prompt { get; set; }
}