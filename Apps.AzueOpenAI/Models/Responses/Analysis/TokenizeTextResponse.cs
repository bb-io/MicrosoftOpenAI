namespace Apps.AzureOpenAI.Models.Responses.Analysis;

public class TokenizeTextResponse
{
    public IEnumerable<int> Tokens { get; set; }
}