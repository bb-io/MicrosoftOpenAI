using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Xliff.Utils.Models;

namespace Apps.AzureOpenAI.Utils;

public static class FileReferenceExtensions
{
    public static XliffType GetXliffType(this FileReference fileReference)
    {
        if (fileReference.Name.EndsWith(".xliff") || fileReference.Name.EndsWith(".xlf"))
        {
            return XliffType.Xliff;
        }
        
        if (fileReference.Name.EndsWith(".mxliff"))
        {
            return XliffType.MXliff;
        }
        
        if (fileReference.Name.EndsWith(".mqxliff"))
        {
            return XliffType.MqXliff;
        }

        throw new Exception($"There is no xliff type for given file name: {fileReference.Name}");
    }
}