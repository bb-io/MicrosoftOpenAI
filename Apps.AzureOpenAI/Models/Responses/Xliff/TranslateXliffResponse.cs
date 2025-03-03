using Apps.AzureOpenAI.Models.Dto;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Response.Xliff;

public class TranslateXliffResponse
{
    public FileReference File { get; set; }
    public UsageDto Usage { get; set; }

    public int Changes { get; set; }
}