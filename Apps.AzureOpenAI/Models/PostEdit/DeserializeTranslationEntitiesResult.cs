using Apps.AzureOpenAI.Models.Entities;

namespace Apps.AzureOpenAI.Models.PostEdit;

public record DeserializeTranslationEntitiesResult(List<TranslationEntity> Translations, bool Success, string Error);
