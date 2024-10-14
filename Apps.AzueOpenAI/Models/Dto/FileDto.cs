using Newtonsoft.Json;

namespace Apps.AzureOpenAI.Models.Dto;

public class FileDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    [JsonProperty("object")]
    public string Object { get; set; } = default!;

    [JsonProperty("bytes")]
    public int Bytes { get; set; }

    [JsonProperty("created_at")]
    public int CreatedAt { get; set; }

    [JsonProperty("filename")]
    public string Filename { get; set; } = default!;

    [JsonProperty("purpose")]
    public string Purpose { get; set; } = default!;

    [JsonProperty("status")]
    public string Status { get; set; } = default!;
}