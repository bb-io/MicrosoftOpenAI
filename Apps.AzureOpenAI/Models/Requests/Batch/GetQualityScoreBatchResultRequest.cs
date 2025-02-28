using Blackbird.Applications.Sdk.Common;

namespace Apps.AzureOpenAI.Models.Requests.Batch;

public class GetQualityScoreBatchResultRequest : GetBatchResultRequest
{
    [Display("Throw error on any unexpected result")]
    public bool? ThrowExceptionOnAnyUnexpectedResult { get; set; }
}