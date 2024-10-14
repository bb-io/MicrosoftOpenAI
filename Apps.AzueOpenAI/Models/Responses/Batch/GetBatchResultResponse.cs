using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Responses.Batch;

public class GetBatchResultResponse
{
    public FileReference File { get; set; } = default!;
}