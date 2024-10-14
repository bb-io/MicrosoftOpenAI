using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Requests.Batch;

public class GetBatchResultRequest : BatchIdentifier
{
    [Display("Original XLIFF")]
    public FileReference OriginalXliff { get; set; } = default!;
}