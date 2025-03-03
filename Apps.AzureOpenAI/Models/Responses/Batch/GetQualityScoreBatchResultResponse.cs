using Apps.AzureOpenAI.Models.Dto;
using Blackbird.Applications.Sdk.Common;

namespace Apps.AzureOpenAI.Models.Responses.Batch;

public class GetQualityScoreBatchResultResponse : GetBatchResultResponse
{
    [Display("Average score")]
    public double AverageScore { get; set; }

    [Display("Usage")]
    public UsageDto Usage { get; set; }
}