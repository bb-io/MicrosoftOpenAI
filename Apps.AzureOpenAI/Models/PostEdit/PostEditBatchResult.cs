using Apps.AzureOpenAI.Models.Dto;
using Apps.AzureOpenAI.Models.Entities;

namespace Apps.AzureOpenAI.Models.PostEdit;

public class PostEditBatchResult
{
    public List<TranslationEntity> UpdatedTranslations { get; set; } = new();
    public UsageDto Usage { get; set; } = new();
    public List<string> ErrorMessages { get; set; } = new();
    public bool IsSuccess { get; set; } = true;
}