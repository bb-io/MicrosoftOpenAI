using Apps.AzureOpenAI.Actions.Base;
using Azure.AI.OpenAI;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.AzureOpenAI.Models.Requests.Image;
using Apps.AzureOpenAI.Models.Responses.Image;
using Apps.AzureOpenAI.Utils;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

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
    }
}
