using Apps.AzureOpenAI.Models.Dto;
using Blackbird.Applications.Sdk.Common;

namespace Apps.AzureOpenAI.Models.Responses.Image
{
    public class ChatContentImageResponse
    {
        [Display("System prompt")]
        public string SystemPrompt { get; set; }

        [Display("User prompt")]
        public string UserPrompt { get; set; }

        [Display("Response")]
        public string Message { get; set; }

        [Display("Usage")]
        public UsageDto Usage { get; set; }
    }
}
