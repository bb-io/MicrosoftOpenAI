using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Text;
using Apps.AzureOpenAI.Models.Requests.Xliff;
using Apps.AzureOpenAI.Actions.Base;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.AzureOpenAI.Models.Response.Xliff;
using MoreLinq;
using Blackbird.Xliff.Utils;
using Blackbird.Xliff.Utils.Extensions;
using Blackbird.Applications.Sdk.Glossaries.Utils.Converters;
using Apps.AzureOpenAI.Models.Dto;
using Apps.AzureOpenAI.Models.Requests.Chat;
using Azure.AI.OpenAI;
using Apps.AzureOpenAI.Utils.Xliff;

namespace Apps.AzureOpenAI.Actions;

[ActionList]
public class XliffActions : BaseActions
{
    private readonly IFileManagementClient _fileManagementClient;

    public XliffActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("Process XLIFF file",
        Description =
            "Processes each translation unit in the XLIFF file according to the provided instructions (by default it just translates the source tags) and updates the target text for each unit. For now it supports only 1.2 version of XLIFF.")]
    public async Task<TranslateXliffResponse> TranslateXliff(
        [ActionParameter] TranslateXliffRequest input,
        [ActionParameter] BaseChatRequest promptRequest,
        [ActionParameter,
         Display("Prompt",
             Description =
                 "Specify the instruction to be applied to each source tag within a translation unit. For example, 'Translate text'")]
        string? prompt,
        [ActionParameter] GlossaryRequest glossary,
        [ActionParameter,
         Display("Bucket size",
             Description =
                 "Specify the number of source texts to be translated at once. Default value: 1500. (See our documentation for an explanation)")]
        int? bucketSize = 1500)
    {
        var fileStream = await _fileManagementClient.DownloadAsync(input.File);
        var xliffDocument = Utils.Xliff.Extensions.ParseXLIFF(fileStream);
        if (xliffDocument.TranslationUnits.Count == 0)
        {
            return new TranslateXliffResponse { File = input.File, Usage = new UsageDto() };
        }

        string systemPrompt = GetSystemPrompt(string.IsNullOrEmpty(prompt));
        var (translatedTexts, usage) = await GetTranslations(prompt, xliffDocument, systemPrompt,
            bucketSize ?? 1500,
            glossary.Glossary, promptRequest);
        //var updatedResults = Utils.Xliff.Extensions.CheckTagIssues(xliffDocument.TranslationUnits, translatedTexts);
        var stream = await _fileManagementClient.DownloadAsync(input.File);
        var updatedFile = Blackbird.Xliff.Utils.Utils.XliffExtensions.UpdateOriginalFile(stream, translatedTexts);
        string contentType = input.File.ContentType ?? "application/xml";
        var fileReference = await _fileManagementClient.UploadAsync(updatedFile, contentType, input.File.Name);
        return new TranslateXliffResponse { File = fileReference, Usage = usage, Changes = translatedTexts.Count };
    }

    [Action("Get Quality Scores for XLIFF file",
        Description = "Gets segment and file level quality scores for XLIFF files")]
    public async Task<ScoreXliffResponse> ScoreXLIFF(
        [ActionParameter] ScoreXliffRequest input, [ActionParameter,
                                                    Display("Prompt",
                                                        Description =
                                                            "Add any linguistic criteria for quality evaluation")]
        string? prompt,
        [ActionParameter] BaseChatRequest promptRequest,
        [ActionParameter,
         Display("Bucket size",
             Description =
                 "Specify the number of translation units to be processed at once. Default value: 1500. (See our documentation for an explanation)")]
        int? bucketSize = 1500)
    {
        var xliffDocument = await LoadAndParseXliffDocument(input.File);
        string criteriaPrompt = string.IsNullOrEmpty(prompt)
            ? "accuracy, fluency, consistency, style, grammar and spelling"
            : prompt;
        var results = new Dictionary<string, float>();
        var batches = xliffDocument.TranslationUnits.Batch((int)bucketSize);
        var src = input.SourceLanguage ?? xliffDocument.SourceLanguage;
        var tgt = input.TargetLanguage ?? xliffDocument.TargetLanguage;

        var usage = new UsageDto();

        foreach (var batch in batches)
        {
            string userPrompt =
                $"Your input is going to be a group of sentences in {src} and their translation into {tgt}. " +
                "Only provide as output the ID of the sentence and the score number as a comma separated array of tuples. " +
                $"Place the tuples in a same line and separate them using semicolons, example for two assessments: 2,7;32,5. The score number is a score from 1 to 10 assessing the quality of the translation, considering the following criteria: {criteriaPrompt}. Sentences: ";
            foreach (var tu in batch)
            {
                userPrompt += $" {tu.Id} {tu.Source} {tu.Target}";
            }

            var systemPrompt =
                "You are a linguistic expert that should process the following texts accoring to the given instructions";
            var (result, promptUsage) = await ExecuteSystemPrompt(promptRequest, userPrompt, systemPrompt);
            usage += promptUsage;

            foreach (var r in result.Split(";"))
            {
                var split = r.Split(",");
                results.Add(split[0], float.Parse(split[1]));
            }
        }

        var file = await _fileManagementClient.DownloadAsync(input.File);
        string fileContent;
        Encoding encoding;
        using (var inFileStream = new StreamReader(file, true))
        {
            encoding = inFileStream.CurrentEncoding;
            fileContent = inFileStream.ReadToEnd();
        }

        foreach (var r in results)
        {
            fileContent = Regex.Replace(fileContent, @"(<trans-unit id=""" + r.Key + @""")",
                @"${1} extradata=""" + r.Value + @"""");
        }

        if (input is { Threshold: not null, Condition: not null, State: not null })
        {
            var filteredTUs = new List<string>();
            switch (input.Condition)
            {
                case ">":
                    filteredTUs = results.Where(x => x.Value > input.Threshold).Select(x => x.Key).ToList();
                    break;
                case ">=":
                    filteredTUs = results.Where(x => x.Value >= input.Threshold).Select(x => x.Key).ToList();
                    break;
                case "=":
                    filteredTUs = results.Where(x => x.Value == input.Threshold).Select(x => x.Key).ToList();
                    break;
                case "<":
                    filteredTUs = results.Where(x => x.Value < input.Threshold).Select(x => x.Key).ToList();
                    break;
                case "<=":
                    filteredTUs = results.Where(x => x.Value <= input.Threshold).Select(x => x.Key).ToList();
                    break;
            }

            fileContent = UpdateTargetState(fileContent, input.State, filteredTUs);
        }

        return new ScoreXliffResponse
        {
            AverageScore = results.Average(x => x.Value),
            File = await _fileManagementClient.UploadAsync(new MemoryStream(encoding.GetBytes(fileContent)),
                MediaTypeNames.Text.Xml, input.File.Name),
            Usage = usage,
        };
    }

    [Action("Post-edit XLIFF file",
        Description = "Updates the targets of XLIFF 1.2 files")]
    public async Task<TranslateXliffResponse> PostEditXLIFF(
        [ActionParameter] PostEditXliffRequest input, [ActionParameter,
                                                       Display("Prompt",
                                                           Description =
                                                               "Additional instructions")]
        string? prompt,
        [ActionParameter] GlossaryRequest glossary,
        [ActionParameter] BaseChatRequest promptRequest,
    [ActionParameter,
         Display("Bucket size",
             Description =
                 "Specify the number of translation units to be processed at once. Default value: 1500. (See our documentation for an explanation)")]
        int? bucketSize = 1500)
    {
        var fileStream = await _fileManagementClient.DownloadAsync(input.File);
        var xliffDocument = Utils.Xliff.Extensions.ParseXLIFF(fileStream);

        var results = new Dictionary<string, string>();
        var batches = xliffDocument.TranslationUnits.Batch((int)bucketSize);
        var src = input.SourceLanguage ?? xliffDocument.SourceLanguage;
        var tgt = input.TargetLanguage ?? xliffDocument.TargetLanguage;
        var usage = new UsageDto();

        foreach (var batch in batches)
        {
            string? glossaryPrompt = null;
            if (glossary?.Glossary != null)
            {
                var glossaryPromptPart =
                    await GetGlossaryPromptPart(glossary.Glossary,
                        string.Join(';', batch.Select(x => x.Source)) + ";" +
                        string.Join(';', batch.Select(x => x.Target)));
                if (glossaryPromptPart != null)
                {
                    glossaryPrompt +=
                        "Enhance the target text by incorporating relevant terms from our glossary where applicable. " +
                        "Ensure that the translation aligns with the glossary entries for the respective languages. " +
                        "If a term has variations or synonyms, consider them and choose the most appropriate " +
                        "translation to maintain consistency and precision. ";
                    glossaryPrompt += glossaryPromptPart;
                }
            }

            var userPrompt =
                $"Your input consists of sentences in {src} language with their translations into {tgt}. " +
                "Review and edit the translated target text as necessary to ensure it is a correct and accurate translation of the source text. " +
                "If you see XML tags in the source also include them in the target text, don't delete or modify them. " +
                "Include only the target texts (updated or not) in the format [ID:X]{target}. " +
                $"Example: [ID:1]{{target1}},[ID:2]{{target2}}. " +
                $"{prompt ?? ""} {glossaryPrompt ?? ""} Sentences: \n" +
                string.Join("\n", batch.Select(tu => $"ID: {tu.Id}; Source: {tu.Source}; Target: {tu.Target}"));

            var systemPrompt =
                "You are a linguistic expert that should process the following texts according to the given instructions";
            var (result, promptUsage) = await ExecuteSystemPrompt(promptRequest, userPrompt, systemPrompt);
            usage += promptUsage;

            var matches = Regex.Matches(result, @"\[ID:(.+?)\]\{([\s\S]+?)\}(?=,\[|$|,?\n)").Cast<Match>().ToList();
            foreach (var match in matches)
            {
                if (match.Groups[2].Value.Contains("[ID:"))
                    continue;
                results.Add(match.Groups[1].Value, match.Groups[2].Value);
            }
        }

        var updatedResults = Utils.Xliff.Extensions.CheckTagIssues(xliffDocument.TranslationUnits, results);
        var originalFile = await _fileManagementClient.DownloadAsync(input.File);
        var updatedFile = Utils.Xliff.Extensions.UpdateOriginalFile(originalFile, updatedResults);

        var finalFile = await _fileManagementClient.UploadAsync(updatedFile, input.File.ContentType, input.File.Name);
        return new TranslateXliffResponse { File = finalFile, Usage = usage, };
    }

    private async Task<XliffDocument> LoadAndParseXliffDocument(FileReference inputFile)
    {
        var stream = await _fileManagementClient.DownloadAsync(inputFile);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream.ToXliffDocument();
    }

    private async Task<string?> GetGlossaryPromptPart(FileReference glossary, string sourceContent)
    {
        var glossaryStream = await _fileManagementClient.DownloadAsync(glossary);
        var blackbirdGlossary = await glossaryStream.ConvertFromTbx();

        var glossaryPromptPart = new StringBuilder();
        glossaryPromptPart.AppendLine();
        glossaryPromptPart.AppendLine();
        glossaryPromptPart.AppendLine("Glossary entries (each entry includes terms in different language. Each " +
                                      "language may have a few synonymous variations which are separated by ;;):");

        var entriesIncluded = false;
        foreach (var entry in blackbirdGlossary.ConceptEntries)
        {
            var allTerms = entry.LanguageSections.SelectMany(x => x.Terms.Select(y => y.Term));
            if (!allTerms.Any(x => Regex.IsMatch(sourceContent, $@"\b{x}\b", RegexOptions.IgnoreCase))) continue;
            entriesIncluded = true;

            glossaryPromptPart.AppendLine();
            glossaryPromptPart.AppendLine("\tEntry:");

            foreach (var section in entry.LanguageSections)
            {
                glossaryPromptPart.AppendLine(
                    $"\t\t{section.LanguageCode}: {string.Join(";; ", section.Terms.Select(term => term.Term))}");
            }
        }

        return entriesIncluded ? glossaryPromptPart.ToString() : null;
    }

    private string UpdateTargetState(string fileContent, string state, List<string> filteredTUs)
    {
        var tus = Regex.Matches(fileContent, @"<trans-unit[\s\S]+?</trans-unit>").Select(x => x.Value);
        foreach (var tu in tus.Where(x =>
                     filteredTUs.Any(y => y == Regex.Match(x, @"<trans-unit id=""(\d+)""").Groups[1].Value)))
        {
            string transformedTU = Regex.IsMatch(tu, @"<target(.*?)state=""(.*?)""(.*?)>")
                ? Regex.Replace(tu, @"<target(.*?state="")(.*?)("".*?)>", @"<target${1}" + state + "${3}>")
                : Regex.Replace(tu, "<target", @"<target state=""" + state + @"""");
            fileContent = Regex.Replace(fileContent, Regex.Escape(tu), transformedTU);
        }

        return fileContent;
    }

    private string GetSystemPrompt(bool translator)
    {
        string prompt;
        if (translator)
        {
            prompt =
                "You are tasked with localizing the provided text. Consider cultural nuances, idiomatic expressions, " +
                "and locale-specific references to make the text feel natural in the target language. " +
                "Ensure the structure of the original text is preserved. Respond with the localized text.";
        }
        else
        {
            prompt =
                "You will be given a list of texts. Each text needs to be processed according to specific instructions " +
                "that will follow. " +
                "The goal is to adapt, modify, or translate these texts as required by the provided instructions. " +
                "Prepare to process each text accordingly and provide the output as instructed.";
        }

        prompt +=
            "Please note that each text is considered as an individual item for translation. Even if there are entries " +
            "that are identical or similar, each one should be processed separately. This is crucial because the output " +
            "should be an array with the same number of elements as the input. This array will be used programmatically, " +
            "so maintaining the same element count is essential.";

        return prompt;
    }

    private async Task<(Dictionary<string, string>, UsageDto)> GetTranslations(string prompt, ParsedXliff xliff,
        string systemPrompt, int bucketSize, FileReference? glossary,
        BaseChatRequest promptRequest)
    {
       
        var results = new List<string>();
        var batches = xliff.TranslationUnits.Batch(bucketSize);

        var usageDto = new UsageDto();
        foreach (var batch in batches)
        {
            string json = JsonConvert.SerializeObject(batch.Select(x => "{ID:" + x.Id + "}" + x.Source));

            var userPrompt = GetUserPrompt(prompt +
                "Reply with the processed text preserving the same format structure as provided, your output will need to be deserialized programmatically afterwards. Do not add linebreaks.",
                xliff, json);

            if (glossary != null)
            {
                var glossaryPromptPart = await GetGlossaryPromptPart(glossary, json);
                if (glossaryPromptPart != null)
                {
                    var glossaryPrompt =
                        "Enhance the target text by incorporating relevant terms from our glossary where applicable. " +
                        "Ensure that the translation aligns with the glossary entries for the respective languages. " +
                        "If a term has variations or synonyms, consider them and choose the most appropriate " +
                        "translation to maintain consistency and precision. ";
                    glossaryPrompt += glossaryPromptPart;
                    userPrompt += glossaryPrompt;
                }
            }

            var (response, promptUsage) = await ExecuteSystemPrompt(promptRequest, userPrompt, systemPrompt);

            usageDto += promptUsage;
            var translatedText = response.Trim();
            string filteredText = "";
            try
            {
                filteredText = Regex.Match(translatedText, "\\[[\\s\\S]+\\]").Value;
                if (String.IsNullOrEmpty(filteredText))
                {
                    var index = translatedText.LastIndexOf("\",") == -1 ? translatedText.LastIndexOf("\"\n,") : translatedText.LastIndexOf("\",");
                    index = index == -1 ? translatedText.LastIndexOf("\n\",") == -1? translatedText.LastIndexOf("\\n\",") : translatedText.LastIndexOf("\n\",") : index;
                    filteredText = translatedText.Remove(index) + "\"]";
                }
                filteredText = Regex.Replace(filteredText,"\\n *", "").Replace("& ", "&amp; ");
                filteredText = Regex.Replace(filteredText, "\\\\n *", "");
                filteredText = Regex.Replace(filteredText,"(\\<(g|x) id=)\\\"(.*?)\\\"\\>", "${1}\"${3}\">");
                filteredText = Regex.Match(filteredText, "\\[[\\s\\S]+\\]").Value;
                var result = JsonConvert.DeserializeObject<string[]>(filteredText);

                results.AddRange(result);
            }
            catch (Exception e)
                {
                    throw new Exception(
                    $"Failed to parse the translated text. Exception message: {e.Message}; Exception type: {e.GetType()}");
            }
                        
        }
       
        return (results.Where(z => Regex.Match(z ,"\\{ID:(.*?)\\}(.+)$").Groups[1].Value != "").ToDictionary(x => Regex.Match(x, "\\{ID:(.*?)\\}(.+)$").Groups[1].Value, y => Regex.Match(y, "\\{ID:(.*?)\\}(.+)$").Groups[2].Value.Trim()), usageDto);

    }

    string GetUserPrompt(string prompt, ParsedXliff xliffDocument, string json)
    {
        string instruction = string.IsNullOrEmpty(prompt)
            ? $"Translate the following texts from {xliffDocument.SourceLanguage} to {xliffDocument.TargetLanguage}."
            : $"Process the following texts as per the custom instructions: {prompt}. The source language is {xliffDocument.SourceLanguage} and the target language is {xliffDocument.TargetLanguage}. This information might be useful for the custom instructions.";

        return
            $"Please provide a translation for each individual text, even if similar texts have been provided more than once. " +
            $"{instruction} Return the outputs as a serialized JSON array of strings without additional formatting " +
            $"(it is crucial because your response will be deserialized programmatically. Please ensure that your response is formatted correctly to avoid any deserialization issues). " +
            $"Original texts (in serialized array format): {json}";
    }

    private XliffDocument UpdateXliffDocumentWithTranslations(XliffDocument xliffDocument, string[] translatedTexts,
        bool updateLockedSegments)
    {
        var updatedUnits = xliffDocument.TranslationUnits.Zip(translatedTexts, (unit, translation) =>
        {
            if (updateLockedSegments == false && unit.Attributes is not null &&
                unit.Attributes.Any(x => x.Key == "locked" && x.Value == "locked"))
            {
                unit.Target = unit.Target;
            }
            else
            {
                unit.Target = translation;
            }

            return unit;
        }).ToList();

        var xDoc = xliffDocument.UpdateTranslationUnits(updatedUnits);
        var stream = new MemoryStream();
        xDoc.Save(stream);
        stream.Position = 0;

        return stream.ToXliffDocument();
        //new XliffConfig{ RemoveWhitespaces = true, CopyAttributes = true, IncludeInlineTags = true }
    }

    private async Task<FileReference> UploadUpdatedDocument(XliffDocument xliffDocument, FileReference originalFile)
    {
        var outputMemoryStream = xliffDocument.ToStream(); //null, false, keepSingleAmpersands: true

        string contentType = originalFile.ContentType ?? "application/xml";
        return await _fileManagementClient.UploadAsync(outputMemoryStream, contentType, originalFile.Name);
    }

    private async Task<(string result, UsageDto usage)> ExecuteSystemPrompt(BaseChatRequest input,
        string prompt,
        string? systemPrompt = null)
    {
        var chatMessages = new List<ChatMessage>();
        if(systemPrompt != null)
        {
            chatMessages.Add(new ChatMessage(ChatRole.System, systemPrompt));
        }
        chatMessages.Add(new ChatMessage(ChatRole.User, prompt));

        var response = await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(DeploymentName, chatMessages)
            {
                MaxTokens = input.MaximumTokens,
                Temperature = input.Temperature,
                PresencePenalty = input.PresencePenalty,
                FrequencyPenalty = input.FrequencyPenalty,
                DeploymentName = DeploymentName,
            });
        var result = response.Value.Choices[0].Message.Content;
        return (result, new(response.Value.Usage));
    }
}
