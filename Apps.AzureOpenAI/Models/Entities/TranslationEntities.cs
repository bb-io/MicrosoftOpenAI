using Newtonsoft.Json;

namespace Apps.AzureOpenAI.Models.Entities;

public class TranslationEntities
{
    [JsonProperty("translations")]
    public List<TranslationEntity> Translations { get; set; } = new();
}