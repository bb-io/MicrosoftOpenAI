using Apps.AzureOpenAI.Models.PostEdit;
using Apps.AzureOpenAI.Models.Requests.Chat;

namespace Apps.AzureOpenAI.Services.Abstract;

public interface IOpenAICompletionService
{
    Task<ChatCompletitionResult> ExecuteChatCompletionAsync(
        IEnumerable<ChatMessageDto> messages, 
        string apiVersion, 
        BaseChatRequest request, 
        object? responseFormat = null);
    
    int CalculateTokenCount(string text, string modelId);
}