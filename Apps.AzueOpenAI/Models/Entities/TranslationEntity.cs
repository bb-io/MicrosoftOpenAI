using Newtonsoft.Json;

namespace Apps.AzureOpenAI.Models.Entities;

public class TranslationEntity
{
    [JsonProperty("translation_id")]
    public string TranslationId { get; set; } = string.Empty;

    [JsonProperty("translated_text")]
    public string TranslatedText { get; set; } = string.Empty;
}