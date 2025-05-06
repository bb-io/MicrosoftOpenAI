using Apps.AzureOpenAI.Models.PostEdit;

namespace Apps.AzureOpenAI.Services.Abstract;

public interface IPostEditService
{
    Task<XliffResult> PostEditXliffAsync(OpenAiXliffInnerRequest request);
    int GetModifiedSegmentsCount(Dictionary<string, string> original, Dictionary<string, string> updated);
}