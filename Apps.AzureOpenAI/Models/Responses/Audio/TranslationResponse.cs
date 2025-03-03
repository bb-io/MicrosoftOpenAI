using Blackbird.Applications.Sdk.Common;

namespace Apps.AzureOpenAI.Models.Responses.Audio;

public class TranslationResponse
{
    [Display("Translated text")]
    public string TranslatedText { get; set; }
}