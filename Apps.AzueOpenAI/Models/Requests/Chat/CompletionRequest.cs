using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class CompletionRequest : BaseChatRequest
{
    public string Prompt { get; set; }
}