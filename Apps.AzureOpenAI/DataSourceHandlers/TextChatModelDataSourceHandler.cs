using Apps.AzureOpenAI.Api;
using Apps.AzureOpenAI.Models.Responses.Batch;
using Apps.AzureOpenAI.Models.Responses.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.AzureOpenAI.DataSourceHandlers;

public class TextChatModelDataSourceHandler(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    async Task<IEnumerable<DataSourceItem>> IAsyncDataSourceItemHandler.GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var restClient = new AzureOpenAIRestClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new AzureOpenAIRequest("/openai/models?api-version=2024-08-01-preview", Method.Get, InvocationContext.AuthenticationCredentialsProviders);

        var models = await restClient.ExecuteWithErrorHandling<ModelsList>(request);
        var modelsDictionary = models.Data
            .Where(model => context.SearchString == null || model.Id.Contains(context.SearchString))
            .Select(model => new DataSourceItem(model.Id,model.Id));
        return modelsDictionary;
    }
}