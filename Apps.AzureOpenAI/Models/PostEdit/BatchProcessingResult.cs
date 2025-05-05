using Apps.AzureOpenAI.Models.Dto;
using Apps.AzureOpenAI.Models.Entities;

namespace Apps.AzureOpenAI.Models.PostEdit;

public record BatchProcessingResult(
    int BatchesProcessed,
    List<TranslationEntity> Results,
    List<UsageDto> Usages,
    List<string> Errors);
