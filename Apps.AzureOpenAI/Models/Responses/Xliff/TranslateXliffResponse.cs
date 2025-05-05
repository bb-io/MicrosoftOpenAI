using Apps.AzureOpenAI.Models.Dto;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Response.Xliff;

public class TranslateXliffResponse
{
    public FileReference File { get; set; } = new();

    public UsageDto Usage { get; set; } = new();

    [Display("Targets updated count")]
    public int Changes { get; set; }
}