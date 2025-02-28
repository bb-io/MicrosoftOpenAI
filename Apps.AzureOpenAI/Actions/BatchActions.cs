using System.Text;
using System.Text.RegularExpressions;
using Apps.AzureOpenAI.Actions.Base;
using Apps.AzureOpenAI.Api;
using Apps.AzureOpenAI.Constants;
using Apps.AzureOpenAI.Models.Dto;
using Apps.AzureOpenAI.Models.Requests.Batch;
using Apps.AzureOpenAI.Models.Responses.Batch;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Xliff.Utils;
using Blackbird.Xliff.Utils.Models;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.AzureOpenAI.Actions;

[ActionList]
public class BatchActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : BaseActions(invocationContext, fileManagementClient)
{
    [Action("(Batch) Process XLIFF file",
        Description =
            "Asynchronously process each translation unit in the XLIFF file according to the provided instructions (by default it just translates the source tags) and updates the target text for each unit. For now it supports only 1.2 version of XLIFF.")]
    public async Task<BatchResponse> ProcessXliffFileAsync([ActionParameter] ProcessXliffFileRequest request)
    {
        var xliffDocument = await DownloadXliffDocumentAsync(request.File);
        var requests = await CreateBatchRequests(xliffDocument, request, (tu, content) =>
            SystemPromptConstants.ProcessXliffFileWithInstructions(
                request.Instructions ?? "Translate the text.",
                string.IsNullOrEmpty(tu.SourceLanguage) ? xliffDocument.SourceLanguage : tu.SourceLanguage,
                string.IsNullOrEmpty(tu.TargetLanguage) ? xliffDocument.TargetLanguage : tu.TargetLanguage));

        return await CreateAndUploadBatchAsync(requests);
    }

    [Action("(Batch) Post-edit XLIFF file",
        Description =
            "Asynchronously post-edit the target text of each translation unit in the XLIFF file according to the provided instructions and updates the target text for each unit. For now it supports only 1.2 version of XLIFF.")]
    public async Task<BatchResponse> PostEditXliffFileAsync([ActionParameter] ProcessXliffFileRequest request)
    {
        var xliffDocument = await DownloadXliffDocumentAsync(request.File);
        var requests = await CreateBatchRequests(xliffDocument, request, (tu, content) =>
            SystemPromptConstants.PostEditXliffFileWithInstructions(
                request.Instructions ?? "Improve the translation",
                string.IsNullOrEmpty(tu.SourceLanguage) ? xliffDocument.SourceLanguage : tu.SourceLanguage,
                string.IsNullOrEmpty(tu.TargetLanguage) ? xliffDocument.TargetLanguage : tu.TargetLanguage));

        return await CreateAndUploadBatchAsync(requests);
    }

    [Action("(Batch) Get Quality Scores for XLIFF file",
        Description = "Asynchronously get quality scores for each translation unit in the XLIFF file.")]
    public async Task<BatchResponse> GetQualityScoresForXliffFileAsync(
        [ActionParameter] ProcessXliffFileRequest request)
    {
        var xliffDocument = await DownloadXliffDocumentAsync(request.File);
        var requests = await CreateBatchRequests(xliffDocument, request, (tu, content) =>
            SystemPromptConstants.EvaluateTranslationQualityWithLanguages(
                string.IsNullOrEmpty(tu.SourceLanguage) ? xliffDocument.SourceLanguage : tu.SourceLanguage,
                string.IsNullOrEmpty(tu.TargetLanguage) ? xliffDocument.TargetLanguage : tu.TargetLanguage));

        return await CreateAndUploadBatchAsync(requests);
    }

    [Action("(Batch) Get results of the batch process",
        Description = "Get the results of the batch process. This action is suitable only for processing and post-editing XLIFF file and should be called after the async process is completed.")]
    public async Task<GetBatchResultResponse> GetBatchResultsAsync([ActionParameter] GetBatchResultRequest request)
    {
        var batchRequests = await GetBatchRequestsAsync(request.BatchId);
        var xliffDocument = await DownloadXliffDocumentAsync(request.OriginalXliff);
        foreach (var batchRequest in batchRequests)
        {
            var translationUnit = xliffDocument.TranslationUnits.Find(tu => tu.Id == batchRequest.CustomId);
            if (translationUnit == null)
            {
                throw new InvalidOperationException(
                    $"Translation unit with id {batchRequest.CustomId} not found in the XLIFF file.");
            }

            var newTargetContent = batchRequest.Response.Body.Choices[0].Message.Content;
            if (request.AddMissingLeadingTrailingTags.HasValue && request.AddMissingLeadingTrailingTags == true)
            {
                var sourceContent = translationUnit.Source;
                    
                var tagPattern = @"<(?<tag>\w+)(?<attributes>[^>]*)>(?<content>.*?)</\k<tag>>";
                var sourceMatch = Regex.Match(sourceContent, tagPattern, RegexOptions.Singleline);

                if (sourceMatch.Success)
                {
                    var tagName = sourceMatch.Groups["tag"].Value;
                    var tagAttributes = sourceMatch.Groups["attributes"].Value;
                    var openingTag = $"<{tagName}{tagAttributes}>";
                    var closingTag = $"</{tagName}>";

                    if (!newTargetContent.Contains(openingTag) && !newTargetContent.Contains(closingTag))
                    {
                        translationUnit.Target = openingTag + newTargetContent + closingTag;
                    }
                    else
                    {
                        translationUnit.Target = newTargetContent;
                    }
                }
                else
                {
                    translationUnit.Target = newTargetContent;
                }
            }
            else
            {
                translationUnit.Target = newTargetContent;
            }
        }

        return new()
        {
            File = await FileManagementClient.UploadAsync(xliffDocument.ToStream(), request.OriginalXliff.ContentType,
                request.OriginalXliff.Name)
        };
    }

    [Action("(Batch) Get quality scores results",
        Description = "Get the quality scores results of the batch process. This action is suitable only for getting quality scores for XLIFF file and should be called after the async process is completed.")]
    public async Task<GetQualityScoreBatchResultResponse> GetQualityScoresResultsAsync(
        [ActionParameter] GetQualityScoreBatchResultRequest request)
    {
        var batchRequests = await GetBatchRequestsAsync(request.BatchId);
        var xliffDocument = await DownloadXliffDocumentAsync(request.OriginalXliff);
        var totalScore = 0d;
        foreach (var batchRequest in batchRequests)
        {
            var translationUnit = xliffDocument.TranslationUnits.Find(tu => tu.Id == batchRequest.CustomId);
            if (translationUnit == null)
            {
                throw new InvalidOperationException(
                    $"Translation unit with id {batchRequest.CustomId} not found in the XLIFF file.");
            }

            if (double.TryParse(batchRequest.Response.Body.Choices[0].Message.Content, out var score))
            {
                totalScore += score;
                translationUnit.Attributes.Add("extradata", batchRequest.Response.Body.Choices[0].Message.Content);
            }
            else if (request.ThrowExceptionOnAnyUnexpectedResult.HasValue &&
                     request.ThrowExceptionOnAnyUnexpectedResult.Value)
            {
                throw new InvalidOperationException(
                    $"The quality score for translation unit with id {batchRequest.CustomId} is not a valid number. " +
                    $"Value: {batchRequest.Response.Body.Choices[0].Message.Content}");
            }
            else
            {
                translationUnit.Attributes.Add("extradata", "0");
            }
        }

        return new()
        {
            File = await FileManagementClient.UploadAsync(xliffDocument.ToStream(), request.OriginalXliff.ContentType,
                request.OriginalXliff.Name),
            AverageScore = totalScore / batchRequests.Count,
        };
    }

    #region Helpers

    private async Task<List<object>> CreateBatchRequests(XliffDocument xliffDocument, ProcessXliffFileRequest request,
        Func<TranslationUnit, string, string> promptGenerator)
    {
        var requests = new List<object>();
        foreach (var translationUnit in xliffDocument.TranslationUnits)
        {
            var content = $"Source: {translationUnit.Source}; Target: {translationUnit.Target}";
            if (request.Glossary != null)
            {
                var glossaryPrompt = GlossaryConstants.GlossaryBeginning +
                                     await GetGlossaryPromptPart(request.Glossary, translationUnit.Source);
                content += $". {glossaryPrompt}";
            }

            var batchRequest = new
            {
                custom_id = translationUnit.Id,
                method = "POST",
                url = "/v1/chat/completions",
                body = new
                {
                    model = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.Deployment).Value,
                    messages = new object[]
                    {
                        new
                        {
                            role = "system",
                            content = promptGenerator(translationUnit, content)
                        },
                        new
                        {
                            role = "user",
                            content
                        }
                    },
                    max_tokens = 4096
                }
            };

            requests.Add(batchRequest);
        }

        return requests;
    }

    private async Task<BatchResponse> CreateAndUploadBatchAsync(List<object> requests)
    {
        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, Encoding.Default);
        foreach (var requestObj in requests)
        {
            var json = JsonConvert.SerializeObject(requestObj);
            await streamWriter.WriteLineAsync(json);
        }

        await streamWriter.FlushAsync();
        memoryStream.Position = 0;

        var bytes = memoryStream.ToArray();

        var uploadFileRequest = new AzureOpenAIRequest("/openai/files?api-version=2024-08-01-preview", Method.Post, InvocationContext.AuthenticationCredentialsProviders)
            .AddFile("file", bytes, $"{Guid.NewGuid()}.jsonl", "application/jsonl")
            .AddParameter("purpose", "batch");
        var file = await RestClient.ExecuteWithErrorHandling<FileDto>(uploadFileRequest);

        do
        {
            await Task.Delay(3000);
            var getFileRequest = new AzureOpenAIRequest($"/openai/files/{file.Id}?api-version=2024-07-01-preview", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            file = await RestClient.ExecuteWithErrorHandling<FileDto>(getFileRequest);
        } while (file.Status == "pending");

        var createBatchRequest = new AzureOpenAIRequest("/openai/batches?api-version=2024-08-01-preview", Method.Post, InvocationContext.AuthenticationCredentialsProviders)
            .WithJsonBody(new
            {
                input_file_id = file.Id,
                endpoint = "/v1/chat/completions",
                completion_window = "24h",
            });
        return await RestClient.ExecuteWithErrorHandling<BatchResponse>(createBatchRequest);
    }
    
    private async Task<List<BatchRequestDto>> GetBatchRequestsAsync(string batchId)
    {
        var getBatchRequest = new AzureOpenAIRequest($"/openai/batches/{batchId}?api-version=2024-08-01-preview", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        var batch = await RestClient.ExecuteWithErrorHandling<BatchResponse>(getBatchRequest);
    
        if (batch.Status != "completed")
        {
            throw new InvalidOperationException(
                $"The batch process is not completed yet. Current status: {batch.Status}");
        }
        
        if(batch.Status == "failed")
        {
            throw new InvalidOperationException(
                $"The batch process failed. Errors: {batch.Errors}");
        }

        var fileContentResponse = await RestClient.ExecuteWithErrorHandling(
            new AzureOpenAIRequest($"/openai/files/{batch.OutputFileId}/content?api-version=2024-08-01-preview", Method.Get, InvocationContext.AuthenticationCredentialsProviders));

        var batchRequests = new List<BatchRequestDto>();
        using var reader = new StringReader(fileContentResponse.Content!);
        while (await reader.ReadLineAsync() is { } line)
        {
            var batchRequest = JsonConvert.DeserializeObject<BatchRequestDto>(line);
            batchRequests.Add(batchRequest);
        }

        return batchRequests;
    }
    
    #endregion
}