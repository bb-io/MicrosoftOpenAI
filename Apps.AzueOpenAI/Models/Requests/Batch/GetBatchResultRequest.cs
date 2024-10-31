using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Requests.Batch;

public class GetBatchResultRequest : BatchIdentifier
{
    [Display("Original XLIFF")]
    public FileReference OriginalXliff { get; set; } = default!;
    
    [Display("Add missing leading/trailing tags", Description = "If true, missing leading tags will be added to the target segment.")]
    public bool? AddMissingLeadingTrailingTags { get; set; }
}