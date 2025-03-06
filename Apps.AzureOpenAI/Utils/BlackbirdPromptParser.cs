using Apps.AzureOpenAI.Models.Responses;
using Azure.AI.OpenAI;

namespace Apps.AzureOpenAI.Utils;

public static class BlackbirdPromptParser
{
    public static (List<ChatMessage>, BlackbirdPromptAdditionalInfo? info) ParseBlackbirdPrompt(string inputPrompt)
    {
        var promptSegments = inputPrompt.Split(";;");

        if (promptSegments.Length is 1)
            return (new() { new(ChatRole.User, promptSegments[0]) }, null);

        if (promptSegments.Length is 2)
            return (new()
            {
                new(ChatRole.System, promptSegments[0]),
                new(ChatRole.User, promptSegments[1])
            }, null);

        if (promptSegments.Length is 3)
            return (new()
            {
                new(ChatRole.System, promptSegments[0]),
                new(ChatRole.User, promptSegments[1])
            }, new BlackbirdPromptAdditionalInfo()
            {
                FileFormat = promptSegments[2]
            });

        throw new("Wrong blackbird prompt format");
    }

    public static string ParseFileFormat(string fileFormat)
    {
        return fileFormat switch
        {
            "Json" => "json_object",
            _ => throw new("Wrong response file format")
        };
    }
}