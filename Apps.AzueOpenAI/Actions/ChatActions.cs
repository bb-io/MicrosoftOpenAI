using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Apps.AzureOpenAI.Extensions;
using Apps.AzureOpenAI.Models.Requests.Chat;
using Apps.AzureOpenAI.Models.Responses.Chat;
using Azure.AI.OpenAI;
using Azure;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Reflection;
using System.Xml.Serialization;
using Apps.AzureOpenAI.Utils;
using Newtonsoft.Json;
using TiktokenSharp;

namespace Apps.AzureOpenAI.Actions;

[ActionList]
public class ChatActions : BaseInvocable
{
    private OpenAIClient Client { get; set; }

    private string DeploymentName { get; set; }

    public ChatActions(InvocationContext invocationContext) : base(invocationContext)
    {
        DeploymentName = InvocationContext.AuthenticationCredentialsProviders.First(x => x.KeyName == "deployment")
            .Value;
        Client = new OpenAIClient(
            new Uri(InvocationContext.AuthenticationCredentialsProviders.First(x => x.KeyName == "url").Value),
            new AzureKeyCredential(InvocationContext.AuthenticationCredentialsProviders
                .First(x => x.KeyName == "apiKey").Value));
    }

    #region Chat actions

    [Action("Generate completion", Description = "Completes the given prompt")]
    public async Task<CompletionResponse> CreateCompletion([ActionParameter] CompletionRequest input)
    {
        var completion = await Client.GetCompletionsAsync(
            new CompletionsOptions(DeploymentName,
                new List<string>() { input.Prompt })
            {
                MaxTokens = input.MaximumTokens,
                Temperature = input.Temperature,
                PresencePenalty = input.PresencePenalty,
                FrequencyPenalty = input.FrequencyPenalty
            });
        return new()
        {
            CompletionText = completion.Value.Choices.First().Text
        };
    }

    [Action("Chat", Description = "Gives a response given a chat message")]
    public async Task<ChatResponse> ChatMessageRequest([ActionParameter] ChatRequest input)
    {
        var response = await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(DeploymentName,
                new List<ChatMessage>() { new ChatMessage(ChatRole.User, input.Message) })
            {
                MaxTokens = input.MaximumTokens,
                Temperature = input.Temperature,
                PresencePenalty = input.PresencePenalty,
                FrequencyPenalty = input.FrequencyPenalty
            });
        return new()
        {
            Message = response.Value.Choices.First().Message.Content
        };
    }

    [Action("Chat with system prompt",
        Description = "Gives a response given a chat message and a configurable system prompt")]
    public async Task<ChatResponse> ChatWithSystemMessageRequest([ActionParameter] SystemChatRequest input)
    {
        var response = await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(DeploymentName,
                new List<ChatMessage>()
                {
                    new ChatMessage(ChatRole.System, input.SystemPrompt), new ChatMessage(ChatRole.User, input.Message)
                })
            {
                MaxTokens = input.MaximumTokens,
                Temperature = input.Temperature,
                PresencePenalty = input.PresencePenalty,
                FrequencyPenalty = input.FrequencyPenalty
            });
        return new()
        {
            Message = response.Value.Choices.First().Message.Content
        };
    }

    [Action("Create summary", Description = "Summarizes the input text")]
    public async Task<SummaryResponse> CreateSummary([ActionParameter] SummaryRequest input)
    {
        var prompt = @$"
                Summarize the following text.

                Text:
                """"""
                {input.Text}
                """"""

                Summary:
            ";

        var completion = await Client.GetCompletionsAsync(
            new CompletionsOptions(DeploymentName,
                new List<string>() { prompt })
            {
                MaxTokens = input.MaximumTokens,
                Temperature = input.Temperature,
                PresencePenalty = input.PresencePenalty,
                FrequencyPenalty = input.FrequencyPenalty
            });

        return new()
        {
            Summary = completion.Value.Choices.First().Text
        };
    }

    [Action("Generate edit", Description = "Edit the input text given an instruction prompt")]
    public async Task<EditResponse> CreateEdit([ActionParameter] EditRequest input)
    {
        var systemPrompt = "You are a text editor. Given provided input text, edit it following the instruction and " +
                           "respond with the edited text.";

        var userPrompt = @$"
                    Input text: {input.InputText}
                    Instruction: {input.Instruction}
                    Edited text:
                    ";
        var response = await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(DeploymentName,
                new List<ChatMessage>()
                    { new ChatMessage(ChatRole.System, systemPrompt), new ChatMessage(ChatRole.User, userPrompt) })
            {
                MaxTokens = input.MaximumTokens,
                Temperature = input.Temperature,
                PresencePenalty = input.PresencePenalty,
                FrequencyPenalty = input.FrequencyPenalty
            });
        return new()
        {
            EditText = response.Value.Choices.First().Message.Content
        };
    }

    [Action("Execute Blackbird prompt", Description = "Execute prompt generated by Blackbird's AI utilities")]
    public async Task<ChatResponse> ExecuteBlackbirdPrompt([ActionParameter] ExecuteBlackbirdPromptRequest input)
    {
        var (messages, _) = BlackbirdPromptParser.ParseBlackbirdPrompt(input.Prompt);

        var response = await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(DeploymentName, messages)
            {
                MaxTokens = input.MaximumTokens,
                Temperature = input.Temperature,
                PresencePenalty = input.PresencePenalty,
                FrequencyPenalty = input.FrequencyPenalty,
            });

        return new()
        {
            Message = response.Value.Choices.First().Message.Content
        };
    }

    #endregion

    #region Translation-related actions

    [Action("Post-edit MT", Description = "Review MT translated text and generate a post-edited version")]
    public async Task<EditResponse> PostEditRequest([ActionParameter] PostEditRequest input)
    {
        var systemPrompt = "You are receiving a source text that was translated by NMT into target text. Review the " +
                           "target text and respond with edits of the target text as necessary. If no edits required, " +
                           "respond with target text.";

        if (input.AdditionalPrompt != null)
            systemPrompt = $"{systemPrompt} {input.AdditionalPrompt}";

        var userPrompt = @$"
            Source text: 
            {input.SourceText}

            Target text: 
            {input.TargetText}
        ";

        var response = await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(DeploymentName,
                new List<ChatMessage>()
                    { new ChatMessage(ChatRole.System, systemPrompt), new ChatMessage(ChatRole.User, userPrompt) }));
        return new()
        {
            EditText = response.Value.Choices.First().Message.Content
        };
    }

    [Action("Get translation issues",
        Description = "Review text translation and generate a comment with the issue description")]
    public async Task<ChatResponse> GetTranslationIssues([ActionParameter] GetTranslationIssuesRequest input)
    {
        var systemPrompt =
            $"You are receiving a source text {(input.SourceLanguage != null ? $"written in {input.SourceLanguage} " : "")}that was translated by NMT into target text {(input.TargetLanguage != null ? $"written in {input.TargetLanguage}" : "")}. " +
            "Review the target text and respond with the issue description.";

        if (input.AdditionalPrompt != null)
            systemPrompt = $"{systemPrompt} {input.AdditionalPrompt}";

        var userPrompt = @$"
            Source text: 
            {input.SourceText}

            Target text: 
            {input.TargetText}
        ";

        var response = await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(DeploymentName,
                new List<ChatMessage>()
                    { new ChatMessage(ChatRole.System, systemPrompt), new ChatMessage(ChatRole.User, userPrompt) })
            {
                MaxTokens = input.MaximumTokens ?? 5000,
                Temperature = input.Temperature ?? 0.5f
            });
        return new()
        {
            Message = response.Value.Choices.First().Message.Content
        };
    }

    [Action("Get MQM report",
        Description = "Perform an LQA Analysis of the translation. The result will be in the MQM framework form.")]
    public async Task<ChatResponse> GetLqaAnalysis([ActionParameter] GetTranslationIssuesRequest input)
    {
        var systemPrompt = "Perform an LQA analysis and use the MQM error typology format using all 7 dimensions. " +
                           "Here is a brief description of the seven high-level error type dimensions: " +
                           "1. Terminology – errors arising when a term does not conform to normative domain or organizational terminology standards or when a term in the target text is not the correct, normative equivalent of the corresponding term in the source text. " +
                           "2. Accuracy – errors occurring when the target text does not accurately correspond to the propositional content of the source text, introduced by distorting, omitting, or adding to the message. " +
                           "3. Linguistic conventions  – errors related to the linguistic well-formedness of the text, including problems with grammaticality, spelling, punctuation, and mechanical correctness. " +
                           "4. Style – errors occurring in a text that are grammatically acceptable but are inappropriate because they deviate from organizational style guides or exhibit inappropriate language style. " +
                           "5. Locale conventions – errors occurring when the translation product violates locale-specific content or formatting requirements for data elements. " +
                           "6. Audience appropriateness – errors arising from the use of content in the translation product that is invalid or inappropriate for the target locale or target audience. " +
                           "7. Design and markup – errors related to the physical design or presentation of a translation product, including character, paragraph, and UI element formatting and markup, integration of text with graphical elements, and overall page or window layout. " +
                           "Provide a quality rating for each dimension from 0 (completely bad) to 10 (perfect). You are an expert linguist and your task is to perform a Language Quality Assessment on input sentences. " +
                           "Try to propose a fixed translation that would have no LQA errors. " +
                           "Formatting: use line spacing between each category. The category name should be bold."
            ;

        if (input.AdditionalPrompt != null)
            systemPrompt = $"{systemPrompt} {input.AdditionalPrompt}";

        var userPrompt =
            $"{(input.SourceLanguage != null ? $"The {input.SourceLanguage} " : "")}\"{input.SourceText}\" was translated as \"{input.TargetText}\"{(input.TargetLanguage != null ? $" into {input.TargetLanguage}" : "")}.{(input.TargetAudience != null ? $" The target audience is {input.TargetAudience}" : "")}";

        var response = await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(DeploymentName,
                new List<ChatMessage>()
                    { new ChatMessage(ChatRole.System, systemPrompt), new ChatMessage(ChatRole.User, userPrompt) })
            {
                MaxTokens = input.MaximumTokens ?? 5000,
                Temperature = input.Temperature ?? 0.5f
            });
        return new()
        {
            Message = response.Value.Choices.First().Message.Content
        };
    }

    [Action("Get MQM dimension values",
        Description =
            "Perform an LQA Analysis of the translation. The result will be in the MQM framework form. This action only returns the scores (between 1 and 10) of each dimension.")]
    public async Task<MqmAnalysis> GetLqaDimensionValues([ActionParameter] GetTranslationIssuesRequest input)
    {
        var systemPrompt = "Perform an LQA analysis and use the MQM error typology format using all 7 dimensions. " +
                           "Here is a brief description of the seven high-level error type dimensions: " +
                           "1. Terminology – errors arising when a term does not conform to normative domain or organizational terminology standards or when a term in the target text is not the correct, normative equivalent of the corresponding term in the source text. " +
                           "2. Accuracy – errors occurring when the target text does not accurately correspond to the propositional content of the source text, introduced by distorting, omitting, or adding to the message. " +
                           "3. Linguistic conventions  – errors related to the linguistic well-formedness of the text, including problems with grammaticality, spelling, punctuation, and mechanical correctness. " +
                           "4. Style – errors occurring in a text that are grammatically acceptable but are inappropriate because they deviate from organizational style guides or exhibit inappropriate language style. " +
                           "5. Locale conventions – errors occurring when the translation product violates locale-specific content or formatting requirements for data elements. " +
                           "6. Audience appropriateness – errors arising from the use of content in the translation product that is invalid or inappropriate for the target locale or target audience. " +
                           "7. Design and markup – errors related to the physical design or presentation of a translation product, including character, paragraph, and UI element formatting and markup, integration of text with graphical elements, and overall page or window layout. " +
                           "Provide a quality rating for each dimension from 0 (completely bad) to 10 (perfect). You are an expert linguist and your task is to perform a Language Quality Assessment on input sentences. " +
                           "Try to propose a fixed translation that would have no LQA errors. " +
                           "The response should be in the following json format: " +
                           "{\r\n  \"terminology\": 0,\r\n  \"accuracy\": 0,\r\n  \"linguistic_conventions\": 0,\r\n  \"style\": 0,\r\n  \"locale_conventions\": 0,\r\n  \"audience_appropriateness\": 0,\r\n  \"design_and_markup\": 0,\r\n  \"proposed_translation\": \"fixed translation\"\r\n}"
            ;

        if (input.AdditionalPrompt != null)
            systemPrompt = $"{systemPrompt} {input.AdditionalPrompt}";

        var userPrompt =
            $"{(input.SourceLanguage != null ? $"The {input.SourceLanguage} " : "")}\"{input.SourceText}\" was translated as \"{input.TargetText}\"{(input.TargetLanguage != null ? $" into {input.TargetLanguage}" : "")}.{(input.TargetAudience != null ? $" The target audience is {input.TargetAudience}" : "")}";

        var response = await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(DeploymentName,
                new List<ChatMessage>()
                    { new ChatMessage(ChatRole.System, systemPrompt), new ChatMessage(ChatRole.User, userPrompt) })
            {
                MaxTokens = input.MaximumTokens ?? 5000,
                Temperature = input.Temperature ?? 0.5f,
            });
        try
        {
            return JsonConvert.DeserializeObject<MqmAnalysis>(response.Value.Choices.First().Message.Content);
        }
        catch
        {
            throw new Exception(
                "Something went wrong parsing the output from OpenAI, most likely due to a hallucination!");
        }
    }

    [Action("Translate text", Description = "Localize the text provided")]
    public async Task<ChatResponse> LocalizeText([ActionParameter] LocalizeTextRequest input)
    {
        var prompt = @$"
                    Original text: {input.Text}
                    Locale: {input.Locale}
                    Localized text:
                    ";
        var tikToken = await TikToken.GetEncodingAsync("cl100k_base");
        var maximumTokensNumber = tikToken.Encode(input.Text).Count + 100;

        var response = await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(DeploymentName,
                new List<ChatMessage>() { new ChatMessage(ChatRole.User, prompt) })
            {
                MaxTokens = maximumTokensNumber,
                Temperature = 0.1f
            });
        return new()
        {
            Message = response.Value.Choices.First().Message.Content
        };
    }

    #endregion
}