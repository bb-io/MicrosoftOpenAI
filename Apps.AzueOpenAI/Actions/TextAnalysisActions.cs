using System.Linq;
using System.Threading.Tasks;
using Apps.AzureOpenAI.Extensions;
using Apps.AzureOpenAI.Models.Requests.Analysis;
using Apps.AzureOpenAI.Models.Responses.Analysis;
using Azure;
using Azure.AI.OpenAI;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using TiktokenSharp;

namespace Apps.AzureOpenAI.Actions;

[ActionList]
public class TextAnalysisActions : BaseInvocable
{
    private OpenAIClient Client { get; set; }

    private string DeploymentName { get; set; }

    public TextAnalysisActions(InvocationContext invocationContext) : base(invocationContext)
    {
        DeploymentName = InvocationContext.AuthenticationCredentialsProviders.First(x => x.KeyName == "deployment").Value;
        Client = new OpenAIClient(
            new Uri(InvocationContext.AuthenticationCredentialsProviders.First(x => x.KeyName == "url").Value),
            new AzureKeyCredential(InvocationContext.AuthenticationCredentialsProviders.First(x => x.KeyName == "apiKey").Value));
    }

    [Action("Create embedding", Description = "Generate an embedding for a text provided. An embedding is a list of " +
                                              "floating point numbers that captures semantic information about the " +
                                              "text that it represents.")]
    public async Task<CreateEmbeddingResponse> CreateEmbedding([ActionParameter] EmbeddingRequest input)
    {
        var embeddings = await Client.GetEmbeddingsAsync(new EmbeddingsOptions() {
                Input = new List<string>() { input.Text },
                DeploymentName = DeploymentName 
        });
        return new()
        {
            Embedding = embeddings.Value.Data.First().Embedding.ToArray(),
        };
    }

    [Action("Tokenize text", Description = "Tokenize the text provided. Optionally specify encoding: cl100k_base " +
                                           "(used by gpt-4, gpt-3.5-turbo, text-embedding-ada-002) or p50k_base " +
                                           "(used by codex models, text-davinci-002, text-davinci-003).")]
    public async Task<TokenizeTextResponse> TokenizeText([ActionParameter] TokenizeTextRequest input)
    {
        var encoding = input.Encoding ?? "cl100k_base";
        var tikToken = await TikToken.GetEncodingAsync(encoding);

        var tokens = tikToken.Encode(input.Text);

        return new()
        {
            Tokens = tokens
        };
    }
}