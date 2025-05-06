using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Requests.Xliff;

public class TranslateXliffRequest
{
    public FileReference File { get; set; }

    [Display("Filter glossary terms")]
    public bool? FilterGlossary { get; set; }

    [Display("Source language")]
    public string? SourceLanguage { get; set; }

    [Display("Target language")]
    public string? TargetLanguage { get; set; }

    [Display("Update locked segments")]
    public bool? UpdateLockedSegments { get; set; }

    [Display("Never fail", Description = "If set to true, the action will ignore any errors and return a result. If set to false, the action will fail if any errors occur.")]
    public bool? NeverFail { get; set; }

    [Display("Batch retry attempts")]
    public int? BatchRetryAttempts { get; set; }
}