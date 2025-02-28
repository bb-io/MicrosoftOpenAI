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

public class ContentFilterResults
{
    [JsonProperty("hate")]
    public FilterItem Hate { get; set; } = new();

    [JsonProperty("self_harm")]
    public FilterItem SelfHarm { get; set; } = new();

    [JsonProperty("sexual")]
    public FilterItem Sexual { get; set; } = new();

    [JsonProperty("violence")]
    public FilterItem Violence { get; set; } = new();
}

public class FilterItem
{
    [JsonProperty("filtered")]
    public bool Filtered { get; set; }

    [JsonProperty("severity")]
    public string Severity { get; set; } = string.Empty;
}

public class ChatChoice
{
    [JsonProperty("finish_reason")]
    public string? FinishReason { get; set; } 
    
    [JsonProperty("index")]
    public int Index { get; set; }

    [JsonProperty("message")] 
    public Message Message { get; set; } = new();
    
    [JsonProperty("content_filter_results")]
    public ContentFilterResults ContentFilterResults { get; set; } = new();
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