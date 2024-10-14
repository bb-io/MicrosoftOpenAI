using Newtonsoft.Json;

namespace Apps.AzureOpenAI.Models.Responses.Models;

public class ModelDto
{
    public string Id { get; set; }
    public string Object { get; set; }
    public int Created { get; set; }
    
    [JsonProperty("owned_by")]
    public string OwnedBy { get; set; }
}

public record ModelsList(IEnumerable<ModelDto> Data);