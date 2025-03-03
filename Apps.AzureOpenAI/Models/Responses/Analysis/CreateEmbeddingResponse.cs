namespace Apps.AzureOpenAI.Models.Responses.Analysis;

public class CreateEmbeddingResponse
{
    public IEnumerable<float> Embedding { get; set; }
}