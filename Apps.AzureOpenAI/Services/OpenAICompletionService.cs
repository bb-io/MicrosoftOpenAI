using Apps.AzureOpenAI.Api;
using Apps.AzureOpenAI.Models.Dto;
using Apps.AzureOpenAI.Models.PostEdit;
using Apps.AzureOpenAI.Models.Requests.Chat;
using Apps.AzureOpenAI.Services.Abstract;
using Blackbird.Applications.Sdk.Common.Authentication;
using Newtonsoft.Json;
using RestSharp;
using TiktokenSharp;

namespace Apps.AzureOpenAI.Services;

public class OpenAICompletionService : IOpenAICompletionService
{
    private const string DefaultEncoding = "cl100k_base";
    private readonly AzureOpenAIRestClient openAIClient;
    private readonly string deploymentName;
    private readonly IEnumerable<AuthenticationCredentialsProvider> creds;

    public OpenAICompletionService(AzureOpenAIRestClient client, IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        this.creds = creds;
        openAIClient = client;
        deploymentName = creds.First(x => x.KeyName == "deployment").Value;
    }

    public async Task<ChatCompletitionResult> ExecuteChatCompletionAsync(
        IEnumerable<ChatMessageDto> messages,
        string apiVersion,
        BaseChatRequest request,
        object? responseFormat = null)
    {
        var endpoint = $"/openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";
        
        var body = new Dictionary<string, object>
        {
            { "messages", messages }
        };

        if (responseFormat != null)
        {
            body.Add("response_format", responseFormat);
        }

        if (request?.Temperature != null)
        {
            body.Add("temperature", request.Temperature.Value);
        }

        if (request?.MaximumTokens != null)
        {
            body.Add("max_completion_tokens", request.MaximumTokens.Value);
        }

        if (request?.PresencePenalty != null)
        {
            body.Add("presence_penalty", request.PresencePenalty.Value);
        }

        if (request?.FrequencyPenalty != null)
        {
            body.Add("frequency_penalty", request.FrequencyPenalty.Value);
        }

        if (request?.TopP != null)
        {
            body.Add("top_p", request.TopP.Value);
        }

        var apiRequest = new AzureOpenAIRequest(endpoint, Method.Post, creds)
            .AddJsonBody(body);

        var response = await openAIClient.ExecuteAsync(apiRequest);

        if (!response.IsSuccessStatusCode)
        {
            if (string.IsNullOrEmpty(response.Content))
            {
                string errorMessage = string.IsNullOrEmpty(response.ErrorMessage)
                    ? "Unexpected error occurred, response from Azure Open AI is empty."
                    : response.ErrorMessage;
                return new(null, false, errorMessage);
            }

            try
            {
                var error = JsonConvert.DeserializeObject<ErrorDto>(response.Content)!;
                return new(null, false, error.ToString());
            }
            catch
            {
                return new(null, false, $"Error: {response.StatusCode} - {response.Content}");
            }
        }

        try
        {
            var chatResponse = JsonConvert.DeserializeObject<OpenAIResponseDto>(response.Content!)!;
            var choice = chatResponse.Choices.FirstOrDefault();

            if (choice == null || string.IsNullOrEmpty(choice.Message?.Content))
            {
                if (choice?.FinishReason == "content_filter")
                {
                    var triggeredFilters = new List<string>();

                    if (choice.ContentFilterResults?.Hate?.Filtered == true)
                    {
                        triggeredFilters.Add($"Hate (severity: {choice.ContentFilterResults.Hate.Severity})");
                    }

                    if (choice.ContentFilterResults?.SelfHarm?.Filtered == true)
                    {
                        triggeredFilters.Add($"Self-harm (severity: {choice.ContentFilterResults.SelfHarm.Severity})");
                    }

                    if (choice.ContentFilterResults?.Sexual?.Filtered == true)
                    {
                        triggeredFilters.Add($"Sexual (severity: {choice.ContentFilterResults.Sexual.Severity})");
                    }

                    if (choice.ContentFilterResults?.Violence?.Filtered == true)
                    {
                        triggeredFilters.Add($"Violence (severity: {choice.ContentFilterResults.Violence.Severity})");
                    }

                    string errorReason;
                    if (triggeredFilters.Count == 0)
                    {
                        errorReason = "Content was blocked by the content filter with no specific category flagged.";
                    }
                    else if (triggeredFilters.Count == 1)
                    {
                        errorReason = $"Content was blocked by the content filter: {triggeredFilters[0]}. ";
                    }
                    else
                    {
                        errorReason = "Content was blocked by multiple filters: " +
                                    string.Join(", ", triggeredFilters) + ". ";
                    }

                    return new(null, false, errorReason);
                }
                else
                {
                    var finishReason = choice?.FinishReason ?? "No finish_reason returned";
                    var errorReason = $"No content returned by the model. (finish_reason: {finishReason})";
                    return new(null, false, errorReason);
                }
            }

            var chatCompletionDto = new ChatCompletionDto(
                chatResponse.Choices.Select(c => new ChatCompletionChoiceDto 
                { 
                    Message = new ChatMessageDto(c.Message.Role, c.Message.Content),
                    FinishReason = c.FinishReason ?? string.Empty
                }),
                new UsageDto(chatResponse.Usage)
            );

            return new(chatCompletionDto, true, null);
        }
        catch (Exception ex)
        {
            return new(null, false, $"Error processing response: {ex.Message}");
        }
    }

    public int CalculateTokenCount(string text, string modelId)
    {
        try
        {
            var encoding = GetEncodingForModel(modelId);
            var tikToken = TikToken.EncodingForModel(encoding);
            return tikToken.Encode(text).Count;
        }
        catch (Exception)
        {
            return (int)Math.Ceiling(text.Length / 4.0);
        }
    }

    private string GetEncodingForModel(string modelId)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            return DefaultEncoding;
        }

        modelId = modelId.ToLower();
        if (modelId.StartsWith("gpt-4") || modelId.StartsWith("gpt-3.5") || modelId.StartsWith("text-embedding"))
        {
            return "cl100k_base";
        }

        if (modelId.Contains("davinci") || modelId.Contains("curie") ||
            modelId.Contains("babbage") || modelId.Contains("ada"))
        {
            return "p50k_base";
        }

        return DefaultEncoding;
    }
}
