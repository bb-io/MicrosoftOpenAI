using Apps.AzureOpenAI.Actions;
using Apps.AzureOpenAI.Models.Requests.Chat;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.AzureOpenAI.Base;

namespace Tests.AzureOpenAI;

[TestClass]
public class ImageActionsTests : TestBase
{
    [TestMethod]
    public async Task GenerateImage_WithValidRequest_ReturnsImageResponse()
    {
        var imageActions = new ImageActions(InvocationContext, FileManager);
        var response = await imageActions.GetLocalizableContentFromImage(
            new GetLocalizableContentFromImageRequest
            {
                Image = new FileReference { Name = "input.png", ContentType= "image/png" }
            }
        );

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }
}