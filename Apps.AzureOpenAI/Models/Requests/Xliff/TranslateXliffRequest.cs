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

    [Display("Never fail", Description = "If set to true, the action will ignore any errors and return a result. If set to false, the action will fail if any errors occur. By default, this is set to false.")]
    public bool? NeverFail { get; set; }

    [Display("Batch retry attempts")]
    public int? BatchRetryAttempts { get; set; }
    
    [Display("Modified by", Description = "The name or ID to use as the modifier. Only works for mxliff files.")]
    public string? ModifiedBy { get; set; }

    [Display("Disable tag checks", Description = "Turn on if you don't want to check if there is any issues with XLIFF file tags. Note that this value can impact the output by only partial file translation")]
    public bool? DisableTagChecks { get; set; } = false;
}