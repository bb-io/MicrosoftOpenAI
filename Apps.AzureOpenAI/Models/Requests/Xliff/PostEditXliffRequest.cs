using Apps.AzureOpenAI.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
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

    [Display("Process only segments with this state", Description = "Only translation units with this value in the target state will be processed"), StaticDataSource(typeof(XliffStateDataSourceHandler))]
    public string? ProcessOnlyTargetState { get; set; }

    [Display("Add missing trailing tags", Description = "If true, missing trailing tags will be added to the target segment.")]
    public bool? AddMissingTrailingTags { get; set; }

    [Display("Never fail", Description = "If true, the request will never fail. Even with the critical error it will simply return the file you sent and the error messages. By default it is set to true.")]
    public bool? NeverFail { get; set; }

    [Display("Batch retry attempts", Description = "The number of attempts to retry the batch in case of failure. By default it is set to 2.")]
    public int? BatchRetryAttempts { get; set; }

    [Display("Max tokens", Description = "The maximum number of tokens to generate in the completion. By default it is set to 1000.")]
    public int? MaxTokens { get; set; }

    [Display("Disable tag checks", Description = "After LLM provide the translation, it will be checked for tags. If the tags are not correct (model hallucinated), the translation of specific translation unit will be rejected. But disabling this option you highly increase the risk of hallucinations. By default it is set to false.")]
    public bool? DisableTagChecks { get; set; }

    [Display("Modified by", Description = "The name to use as the modifier. Only works for mxliff files.")]
    public string? ModifiedBy { get; set; }
}