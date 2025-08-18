using Apps.AzureOpenAI.Actions.Base;
using Azure.AI.OpenAI;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.AzureOpenAI.Models.Requests.Image;
using Apps.AzureOpenAI.Models.Responses.Image;
using Apps.AzureOpenAI.Utils;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Apps.AzureOpenAI.Models.Dto;
using Apps.AzureOpenAI.Models.Requests.Chat;
using Apps.AzureOpenAI.Models.Responses.Chat;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Apps.AzureOpenAI.Api;
using RestSharp;

namespace Apps.AzureOpenAI.Actions
{
    [ActionList]
    public class ImageActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : BaseActions(invocationContext, fileManagementClient)
    {
        [Action("Generate image", Description = "Generates an image based on a prompt")]
        public async Task<ImageResponse> GenerateImage([ActionParameter] ImageRequest input)
        {
            var images = await TryCatchHelper.ExecuteWithErrorHandling(() => Client.GetImageGenerationsAsync(new ImageGenerationOptions(input.Prompt)
            {
                Size = input.Size,
                ImageCount = 1
            }));

            return new()
            {
                Url = images.Value.Data.First().Url.ToString()
            };
        }

        [Action("Get localizable content from image",
            Description = "Extracts localizable text from an image ")]
        public async Task<ChatContentImageResponse> GetLocalizableContentFromImage(
            [ActionParameter] GetLocalizableContentFromImageRequest input)
        {
            var systemPrompt =
            "Your objective is to perform OCR and return ONLY the localizable text found in the image. " +
            "Do not describe the image, and do not add extra characters. If there is no text, return an empty string.";

            using var imgStream = await FileManagementClient.DownloadAsync(input.Image);
            var imgBytes = await imgStream.GetByteData();
            var dataUrl = $"data:{input.Image.ContentType};base64,{Convert.ToBase64String(imgBytes)}";

            var deploymentName = InvocationContext.AuthenticationCredentialsProviders.Get("deployment").Value;
            var apiVersion = "2024-08-01-preview";

            var payload = new
            {
                messages = new object[]
                {
                new { role = "system", content = systemPrompt },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = "Extract text" },
                        new { type = "image_url", image_url = new { url = dataUrl } }
                    }
                }
                },
                temperature = input.Temperature ?? 0,
                max_tokens = input.MaximumTokens ?? 1000
            };

            var client = new AzureOpenAIRestClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new AzureOpenAIRequest($"/openai/deployments/{deploymentName}/chat/completions",Method.Post,InvocationContext.AuthenticationCredentialsProviders);

            request.AddQueryParameter("api-version", apiVersion);
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(payload);

            var parsed = await client.ExecuteWithErrorHandling<ChatCompletionsResponse>(request);

            return new ChatContentImageResponse
            {
                SystemPrompt = systemPrompt,
                UserPrompt = string.Empty,
                Message = parsed?.choices?.FirstOrDefault()?.message?.content ?? string.Empty,
                Usage = new UsageDto
                {
                    PromptTokens = parsed?.usage?.prompt_tokens ?? 0,
                    CompletionTokens = parsed?.usage?.completion_tokens ?? 0,
                    TotalTokens = parsed?.usage?.total_tokens ?? 0
                }
            };
        }
    }
}
