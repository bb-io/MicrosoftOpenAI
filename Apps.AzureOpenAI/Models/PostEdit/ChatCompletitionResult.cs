
namespace Apps.AzureOpenAI.Models.PostEdit;

public record class ChatCompletitionResult(ChatCompletionDto? ChatCompletion, bool Success, string? Error = null);