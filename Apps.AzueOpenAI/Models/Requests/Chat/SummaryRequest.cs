namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class SummaryRequest : BaseChatRequest
{
    public string Text { get; set; }
}