using Newtonsoft.Json;

namespace Apps.AzureOpenAI.Models.Dto;

public class OpenAIResponseDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("created")]
    public long Created { get; set; }
    
    [JsonProperty("model")]
    public string Model { get; set; } = string.Empty;
    
    [JsonProperty("object")]
    public string Object { get; set; } = string.Empty;
    
    [JsonProperty("choices")]
    public List<ChatChoice> Choices { get; set; } = new();
    
    [JsonProperty("usage")]
    public CompletionsUsage Usage { get; set; } = new();
}

public class ChatChoice
{
    [JsonProperty("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
    
    [JsonProperty("index")]
    public int Index { get; set; }
    
    [JsonProperty("message")]
    public Message Message { get; set; } = new();
}

public class Message
{
    [JsonProperty("content"),]
    public string Content { get; set; } = string.Empty;
    
    [JsonProperty("role")]
    public string Role { get; set; } = string.Empty;
}

public class CompletionsUsage
{
    [JsonProperty("completion_tokens")]
    public int CompletionTokens { get; set; }
    
    [JsonProperty("prompt_tokens")]
    public int PromptTokens { get; set; }
    
    [JsonProperty("total_tokens")]
    public int TotalTokens { get; set; }
}