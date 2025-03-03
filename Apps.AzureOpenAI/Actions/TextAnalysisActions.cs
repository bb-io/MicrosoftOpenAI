using Apps.AzureOpenAI.Actions.Base;
using Apps.AzureOpenAI.Models.Requests.Analysis;
using Apps.AzureOpenAI.Models.Responses.Analysis;
using Apps.AzureOpenAI.Utils;
using Azure.AI.OpenAI;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using TiktokenSharp;

namespace Apps.AzureOpenAI.Actions;

[ActionList]
public class TextAnalysisActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : BaseActions(invocationContext, fileManagementClient)
{
    [Action("Create embedding", Description = "Generate an embedding for a text provided. An embedding is a list of " +
                                              "floating point numbers that captures semantic information about the " +
                                              "text that it represents.")]
    public async Task<CreateEmbeddingResponse> CreateEmbedding([ActionParameter] EmbeddingRequest input)
    {
        var embeddings = await TryCatchHelper.ExecuteWithErrorHandling(async () => await EmbeddingClient.GenerateEmbeddingsAsync(new List<string>() { input.Text }));
        
        return new()
        {
            Embedding = embeddings.Value.First().ToFloats().ToArray()
            //Embedding = embeddings.Value.Data.First().Embedding.ToArray(),
        };
    }

    [Action("Tokenize text", Description = "Tokenize the text provided. Optionally specify encoding: cl100k_base " +
                                           "(used by gpt-4, gpt-3.5-turbo, text-embedding-ada-002) or p50k_base " +
                                           "(used by codex models, text-davinci-002, text-davinci-003).")]
    public async Task<TokenizeTextResponse> TokenizeText([ActionParameter] TokenizeTextRequest input)
    {
        var encoding = input.Encoding ?? "cl100k_base";
        var tikToken = await TryCatchHelper.ExecuteWithErrorHandling(() => TikToken.GetEncodingAsync(encoding));

        var tokens = tikToken.Encode(input.Text);
        return new()
        {
            Tokens = tokens
        };
    }
}