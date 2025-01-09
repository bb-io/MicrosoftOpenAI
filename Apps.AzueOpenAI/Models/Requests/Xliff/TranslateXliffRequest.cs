using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Requests.Xliff;

public class TranslateXliffRequest
{
    public FileReference File { get; set; }

    [Display("Filter glossary terms")]
    public bool? FilterGlossary { get; set; }
}