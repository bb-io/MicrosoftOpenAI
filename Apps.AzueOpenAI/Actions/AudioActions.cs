using Azure.AI.OpenAI;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.AzureOpenAI.Actions.Base;
using Apps.AzureOpenAI.Models.Requests.Audio;
using Apps.AzureOpenAI.Models.Responses.Audio;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;

namespace Apps.AzureOpenAI.Actions
{
    public class AudioActions : BaseActions
    {
        private readonly IFileManagementClient _fileManagementClient;
        
        public AudioActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
            : base(invocationContext)
        {
            _fileManagementClient = fileManagementClient;
        }

        [Action("Create English translation", Description = "Generates a translation into English given an audio or " +
                                                            "video file (mp3, mp4, mpeg, mpga, m4a, wav, or webm).")]
        public async Task<TranslationResponse> CreateTranslation([ActionParameter] TranslationRequest input)
        {
            var fileStream = await _fileManagementClient.DownloadAsync(input.File);
            var fileBytes = await fileStream.GetByteData();
            var translation = await Client.GetAudioTranslationAsync(
                new AudioTranslationOptions(DeploymentName, new BinaryData(fileBytes))
                {
                    Prompt = input.Prompt,
                    Temperature = input.Temperature,
                    ResponseFormat = AudioTranslationFormat.Verbose
                });
            return new()
            {
                TranslatedText = translation.Value.Text
            };
        }

        [Action("Create transcription", Description = "Generates a transcription given an audio or video file (mp3, " +
                                                      "mp4, mpeg, mpga, m4a, wav, or webm).")]
        public async Task<TranscriptionResponse> CreateTranscription([ActionParameter] TranscriptionRequest input)
        {
            var fileStream = await _fileManagementClient.DownloadAsync(input.File);
            var fileBytes = await fileStream.GetByteData();
            var transcription = await Client.GetAudioTranscriptionAsync(
                new AudioTranscriptionOptions(DeploymentName, new BinaryData(fileBytes))
                {
                    Language = input.Language,
                    Prompt = input.Prompt,
                    Temperature = input.Temperature,
                    ResponseFormat = AudioTranscriptionFormat.Verbose
                });
            return new()
            {
                Transcription = transcription.Value.Text
            };
        }
    }
}
