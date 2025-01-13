using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Requests.Xliff;

public class PostEditXliffRequest
{
    public FileReference File { get; set; }

    [Display("Source language")]
    public string? SourceLanguage { get; set; }

    [Display("Target language")]
    public string? TargetLanguage { get; set; }

    [Display("Update locked segments", Description = "By default it set to false. If false, Azure OpenAI will not change the segments that are locked in the XLIFF file.")]
    public bool? PostEditLockedSegments { get; set; }

    [Display("Filter glossary terms")]
    public bool? FilterGlossary { get; set; }
}