using Apps.AzureOpenAI.Models.PostEdit;
using Apps.AzureOpenAI.Models.Response.Xliff;
using Blackbird.Applications.Sdk.Common;

namespace Apps.AzureOpenAI.Models.Responses.Xliff;

public class ProcessXliffResponse : TranslateXliffResponse
{
    [Display("Total segments count")]
    public double TotalSegmentsCount { get; set; }

    [Display("Processed batches count")]
    public double ProcessedBatchesCount { get; set; }

    [Display("Error messages count")]
    public double ErrorMessagesCount { get; set; }

    [Display("Error messages")]
    public List<string> ErrorMessages { get; set; } = new();

    [Display("Locked segments exclude count")]
    public double LockedSegmentsExcludeCount { get; set; }

    public ProcessXliffResponse(XliffResult postEditResult) 
    {
        File = postEditResult.File;
        Usage = postEditResult.Usage;
        Changes = postEditResult.TargetsUpdatedCount;
        ProcessedBatchesCount = postEditResult.ProcessedBatchesCount;
        TotalSegmentsCount = postEditResult.TotalSegmentsCount;
        ErrorMessagesCount = postEditResult.ErrorMessages.Count;
        ErrorMessages = postEditResult.ErrorMessages;
        LockedSegmentsExcludeCount = postEditResult.LockedSegmentsExcludeCount;
    }
}