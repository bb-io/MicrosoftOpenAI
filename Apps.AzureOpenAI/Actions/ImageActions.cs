using Apps.AzureOpenAI.Actions.Base;
using Azure.AI.OpenAI;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.AzureOpenAI.Models.Requests.Image;
using Apps.AzureOpenAI.Models.Responses.Image;
using Apps.AzureOpenAI.Utils;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using OpenAI.Images;

namespace Apps.AzureOpenAI.Actions
{
    [ActionList]
    public class ImageActions : BaseActions
    {
        protected readonly ImageClient ImageClient;

        public ImageActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient):base(invocationContext, fileManagementClient)
        {
            ImageClient = Client.GetImageClient(DeploymentName);
        }

        [Action("Generate image", Description = "Generates an image based on a prompt")]
        public async Task<ImageResponse> GenerateImage([ActionParameter] ImageRequest input)
        {
            var images = await TryCatchHelper.ExecuteWithErrorHandling(() => ImageClient.GenerateImageAsync(input.Prompt));
            
            return new()
            {
                Url = images.Value.ImageUri.ToString()
            };
        }
    }
}
