using Apps.AzureOpenAI.Api;
using Apps.AzureOpenAI.Models.Responses.Batch;
using Apps.AzureOpenAI.Models.Responses.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.AzureOpenAI.DataSourceHandlers;

public class TextChatModelDataSourceHandler(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var restClient = new AzureOpenAiRestClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new AzureOpenAiRequest("/openai/models?api-version=2024-08-01-preview", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        
        var models = await restClient.ExecuteWithErrorHandling<ModelsList>(request);
        var modelsDictionary = models.Data
            .Where(model => context.SearchString == null || model.Id.Contains(context.SearchString))
            .ToDictionary(model => model.Id, model => model.Id);
        return modelsDictionary;
    }
}