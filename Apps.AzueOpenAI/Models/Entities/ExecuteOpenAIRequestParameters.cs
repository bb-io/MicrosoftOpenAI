using Apps.AzureOpenAI.Models.Requests.Chat;

namespace Apps.AzureOpenAI.Models.Entities;

public record ExecuteOpenAIRequestParameters(string Prompt, string SystemPrompt, string ApiVersion, BaseChatRequest ChatRequest, object? ResponseFormat);