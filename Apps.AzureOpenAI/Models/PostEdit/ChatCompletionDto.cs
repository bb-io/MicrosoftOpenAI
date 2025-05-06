using Apps.AzureOpenAI.Models.Dto;
using Newtonsoft.Json;

namespace Apps.AzureOpenAI.Models.PostEdit;

public record ChatCompletionDto(IEnumerable<ChatCompletionChoiceDto> Choices, UsageDto Usage);

public record ChatCompletionChoiceDto
{
    public ChatMessageDto Message { get; init; }

    [JsonProperty("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
}

public record BaseChatMessageDto(string Role);
public record ChatMessageDto(string Role, string Content) : BaseChatMessageDto(Role);