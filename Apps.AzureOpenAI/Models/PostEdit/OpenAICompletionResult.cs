using Apps.AzureOpenAI.Models.Dto;
using Apps.AzureOpenAI.Models.Entities;

namespace Apps.AzureOpenAI.Models.PostEdit;

public record OpenAICompletionResult(
    bool IsSuccess,
    UsageDto Usage,
    List<string> Errors,
    List<TranslationEntity> Translations);
