using Blackbird.Applications.Sdk.Common;

namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class EditRequest : BaseChatRequest
{
    [Display("Input text")]
    public string InputText { get; set; }

    public string Instruction { get; set; }
}