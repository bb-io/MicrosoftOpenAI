using Newtonsoft.Json;

namespace Apps.AzureOpenAI.Models.Dto;

public class ErrorDto
{
    [JsonProperty("error")]
    public Error Error { get; set; } = new();

    public override string ToString()
    {
        return $"We encountered an error. Error code: {Error.Code}; Error message: {Error.Message}";
    }
}

public class Error
{
    [JsonProperty("code")]
    public string Code { get; set; } = string.Empty;
    
    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;
}