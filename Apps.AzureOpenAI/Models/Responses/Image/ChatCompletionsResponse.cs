

using Blackbird.Applications.Sdk.Common;

namespace Apps.AzureOpenAI.Models.Responses.Image
{
    public class ChatCompletionsResponse
    {
        [Display("Choices")]
        public Choice[] choices { get; set; }

        [Display("Usage")]
        public Usage usage { get; set; }
    }

    public class Choice
    {
        [Display("Message")]
        public Message message { get; set; }
    }
    public class Message
    {
        [Display("Role")]
        public string role { get; set; }

        [Display("Content")]
        public string content { get; set; }
    }

    public class Usage
    {
        [Display("Prompt tokens")]
        public int prompt_tokens { get; set; }

        [Display("Completion tokens")]
        public int completion_tokens { get; set; }

        [Display("Total tokens")]
        public int total_tokens { get; set; }
    }
}
