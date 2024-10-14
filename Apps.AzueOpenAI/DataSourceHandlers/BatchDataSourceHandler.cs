using Apps.AzureOpenAI.Api;
using Apps.AzureOpenAI.Models.Responses.Batch;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.AzureOpenAI.DataSourceHandlers;

public class BatchDataSourceHandler(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var restClient = new AzureOpenAiRestClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new AzureOpenAiRequest("/openai/batches?api-version=2024-08-01-preview", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        
        var batches = await restClient.ExecuteWithErrorHandling<BatchPaginationResponse>(request);
        var modelsDictionary = batches.Data
            .Where(model => context.SearchString == null || model.Id.Contains(context.SearchString))
            .ToDictionary(model => model.Id, model => model.Id);
        return modelsDictionary;
    }
}