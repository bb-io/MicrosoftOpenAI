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
using Apps.AzureOpenAI.Constants;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.AzureOpenAI.Models.Response.Xliff;
using MoreLinq;
using Blackbird.Xliff.Utils;
using Blackbird.Xliff.Utils.Extensions;
using Blackbird.Applications.Sdk.Glossaries.Utils.Converters;
using Apps.AzureOpenAI.Models.Dto;
using Apps.AzureOpenAI.Models.Entities;
using Apps.AzureOpenAI.Models.Requests.Chat;
using Apps.AzureOpenAI.Utils;
using Azure.AI.OpenAI;

namespace Apps.AzureOpenAI.Actions;

[ActionList]
public class XliffActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : BaseActions(invocationContext, fileManagementClient)
{
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
        var xliffDocument = await DownloadXliffDocumentAsync(input.File);
        if (xliffDocument.TranslationUnits.Count == 0)
        {
            return new TranslateXliffResponse { File = input.File, Usage = new UsageDto() };
        }

        var systemPrompt = GetSystemPrompt(string.IsNullOrEmpty(prompt));
        var (translatedTexts, usage) = await ProcessTranslationUnits(xliffDocument,
            new(prompt, systemPrompt, bucketSize ?? 1500, promptRequest, glossary?.Glossary, input.FilterGlossary));

        translatedTexts.ForEach(x =>
        {
            var translationUnit = xliffDocument.TranslationUnits.FirstOrDefault(tu => tu.Id == x.TranslationId);
            if (translationUnit != null)
            {
                translationUnit.Target = x.TranslatedText;
            }
        });

        var fileReference =
            await fileManagementClient.UploadAsync(xliffDocument.ToStream(), input.File.ContentType, input.File.Name);
        return new TranslateXliffResponse { File = fileReference, Usage = usage };
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
        var xliffDocument = await DownloadXliffDocumentAsync(input.File);
        string criteriaPrompt = string.IsNullOrEmpty(prompt)
            ? "accuracy, fluency, consistency, style, grammar and spelling"
            : prompt;
        
        var batches = xliffDocument.TranslationUnits.Batch((int)bucketSize);
        var src = input.SourceLanguage ?? xliffDocument.SourceLanguage;
        var tgt = input.TargetLanguage ?? xliffDocument.TargetLanguage;
        
        var usage = new UsageDto();
        var results = new Dictionary<string, float>();
        foreach (var batch in batches)
        {
            var userPrompt = PromptConstants.GetQualityScorePrompt(criteriaPrompt, src, tgt,
                JsonConvert.SerializeObject(batch.Select(x => new { x.Id, x.Source, x.Target }).ToList()));
            var (result, promptUsage) = await ExecuteOpenAIRequestAsync(new(userPrompt, PromptConstants.DefaultSystemPrompt, "2024-08-01-preview",
                promptRequest, ResponseFormats.GetQualityScoreXliffResponseFormat()));
            usage += promptUsage;
            
            TryCatchHelper.TryCatch(() =>
            {
                var deserializeResult = JsonConvert.DeserializeObject<TranslationEntities>(result)!;
                foreach (var entity in deserializeResult.Translations)
                {
                    results.Add(entity.TranslationId, entity.QualityScore);
                }
            }, $"Failed to deserialize the response from OpenAI, try again later. Response: {result}");
        }
        
        results.ForEach(x =>
        {
            var translationUnit = xliffDocument.TranslationUnits.FirstOrDefault(tu => tu.Id == x.Key);
            if (translationUnit != null)
            {
                var attribute = translationUnit.Attributes.FirstOrDefault(x => x.Key == "extradata");
                if (!string.IsNullOrEmpty(attribute.Key))
                {
                    translationUnit.Attributes.Remove(attribute.Key);
                    translationUnit.Attributes.Add("extradata", x.Value.ToString());
                }
                else
                {
                    translationUnit.Attributes.Add("extradata", x.Value.ToString());
                }
            }
        });

        if (input.Threshold != null && input.Condition != null && input.State != null)
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
            
            filteredTUs.ForEach(x =>
            {
                var translationUnit = xliffDocument.TranslationUnits.FirstOrDefault(tu => tu.Id == x);
                if (translationUnit != null)
                {
                    var stateAttribute = translationUnit.Attributes.FirstOrDefault(x => x.Key == "state");
                    if (!string.IsNullOrEmpty(stateAttribute.Key))
                    {
                        translationUnit.Attributes.Remove(stateAttribute.Key);
                        translationUnit.Attributes.Add("state", input.State);
                    }
                    else
                    {
                        translationUnit.Attributes.Add("state", input.State);
                    }
                }
            });
        }

        var stream = xliffDocument.ToStream();
        return new ScoreXliffResponse
        {
            AverageScore = results.Average(x => x.Value),
            File = await FileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Xml, input.File.Name),
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
        var xliffDocument = await DownloadXliffDocumentAsync(input.File);

        var batches = xliffDocument.TranslationUnits.Batch((int)bucketSize!).ToList();
        var src = input.SourceLanguage ?? xliffDocument.SourceLanguage;
        var tgt = input.TargetLanguage ?? xliffDocument.TargetLanguage;
        var usage = new UsageDto();

        var results = new List<TranslationEntity>();
        foreach (var batch in batches)
        {
            var filteredBatch = input.PostEditLockedSegments == true
                ? batch 
                : batch.Where(x => !x.IsLocked()).ToArray();
            
            var glossaryPrompt = string.Empty;
            if (glossary?.Glossary != null)
            {
                var glossaryStream = await FileManagementClient.DownloadAsync(glossary.Glossary);
                var blackbirdGlossary = await glossaryStream.ConvertFromTbx();
                glossaryPrompt = GlossaryPrompts.GetGlossaryPromptPart(blackbirdGlossary,
                    string.Join(';', filteredBatch.Select(x => x.Source)), input.FilterGlossary);
                if (!string.IsNullOrEmpty(glossaryPrompt))
                {
                    glossaryPrompt +=
                        "Enhance the target text by incorporating relevant terms from our glossary where applicable. " +
                        "Ensure that the translation aligns with the glossary entries for the respective languages. " +
                        "If a term has variations or synonyms, consider them and choose the most appropriate " +
                        "translation to maintain consistency and precision. ";
                }
            }

            var json = JsonConvert.SerializeObject(filteredBatch.Select(x => new { x.Id, x.Source, x.Target }).ToList());
            var userPrompt = PromptConstants.GetPostEditPrompt(prompt, glossaryPrompt, src, tgt,
                json);

            var (result, promptUsage) = await ExecuteOpenAIRequestAsync(new(userPrompt, PromptConstants.DefaultSystemPrompt,
                "2024-08-01-preview", promptRequest, ResponseFormats.GetProcessXliffResponseFormat()));
            usage += promptUsage;

            TryCatchHelper.TryCatch(() =>
            {
                var deserializedTranslations = JsonConvert.DeserializeObject<TranslationEntities>(result)!;
                results.AddRange(deserializedTranslations.Translations);
            }, $"Failed to deserialize the response from OpenAI, try again later. Response: {result}");
        }

        var changes = 0;
        results.ForEach(x =>
        {
            var translationUnit = xliffDocument.TranslationUnits.FirstOrDefault(tu => tu.Id == x.TranslationId);
            if (translationUnit != null)
            {
                if (translationUnit.Target != x.TranslatedText)
                {
                    changes += 1;
                }
                
                translationUnit.Target = x.TranslatedText;
            }
        });

        var fileReference =
            await FileManagementClient.UploadAsync(xliffDocument.ToStream(), input.File.ContentType, input.File.Name);
        return new TranslateXliffResponse { File = fileReference, Usage = usage, Changes = changes};
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

    private async Task<(List<TranslationEntity>, UsageDto)> ProcessTranslationUnits(XliffDocument xliff,
        XliffParameters parameters)
    {
        var batches = xliff.TranslationUnits.Batch(parameters.BucketSize);

        var usageDto = new UsageDto();
        var entities = new List<TranslationEntity>();
        foreach (var batch in batches)
        {
            var json = JsonConvert.SerializeObject(batch.Select(x => new { x.Id, x.Source }).ToList());
            var prompt = PromptConstants.GetProcessPrompt(parameters.Prompt, xliff.SourceLanguage,
                xliff.TargetLanguage, json);

            if (parameters.Glossary != null)
            {
                var glossaryStream = await FileManagementClient.DownloadAsync(parameters.Glossary);
                var blackbirdGlossary = await glossaryStream.ConvertFromTbx();
                var glossaryPromptPart = GlossaryPrompts.GetGlossaryPromptPart(blackbirdGlossary, json, parameters.filterTerms);
                prompt = GlossaryPrompts.GetGlossaryWithUserPrompt(prompt, glossaryPromptPart);
            }

            var (response, promptUsage) =
                await ExecuteOpenAIRequestAsync(new(prompt, parameters.SystemPrompt, "2024-08-01-preview",
                    parameters.ChatRequest, ResponseFormats.GetProcessXliffResponseFormat()));
            usageDto += promptUsage;

            var translatedText = response.Trim();
            TryCatchHelper.TryCatch(() =>
            {
                var deserializedTranslations = JsonConvert.DeserializeObject<TranslationEntities>(translatedText)!;
                entities.AddRange(deserializedTranslations.Translations);
            }, $"Failed to deserialize the response from OpenAI, try again later. Response: {translatedText}");
        }

        return (entities, usageDto);
    }
}