using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class GetTranslationIssuesRequest
{
    [Display("Source text")]
    public string SourceText { get; set; }

    [Display("Target text")]
    public string TargetText { get; set; }

    [Display("Additional prompt")]
    public string? AdditionalPrompt { get; set; }

    [Display("Source language")]
    public string? SourceLanguage { get; set; }

    [Display("Target language")]
    public string? TargetLanguage { get; set; }

    [Display("Maximum tokens")]
    public int? MaximumTokens { get; set; }

    [Display("Temperature")]
    [StaticDataSource(typeof(TemperatureDataSourceHandler))]
    public float? Temperature { get; set; }

    [Display("Target audience")]
    public string? TargetAudience { get; set; }
}
