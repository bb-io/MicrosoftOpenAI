using Apps.AzureOpenAI.Actions.Base;
using Apps.AzureOpenAI.Api;
using Apps.AzureOpenAI.Models.Requests.Batch;
using Apps.AzureOpenAI.Models.Responses.Batch;
using Apps.AzureOpenAI.Polling.Models;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using RestSharp;

namespace Apps.AzureOpenAI.Polling;

[PollingEventList]
public class BatchPollingList(InvocationContext invocationContext) : BaseActions(invocationContext, null!)
{
    [PollingEvent("On batch finished", "Triggered when a batch status is set to completed")]
    public async Task<PollingEventResponse<BatchMemory, BatchResponse>> OnBatchFinished(
        PollingEventRequest<BatchMemory> request,
        [PollingEventParameter] BatchIdentifier identifier)
    {
        if (request.Memory is null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastPollingTime = DateTime.UtcNow,
                    Triggered = false
                }
            };
        }
        
        var getBatchRequest = new AzureOpenAIRequest($"/batches/{identifier.BatchId}", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        var batch = await RestClient.ExecuteWithErrorHandling<BatchResponse>(getBatchRequest);
        var triggered = batch.Status == "completed" && !request.Memory.Triggered;
        return new()
        {
            FlyBird = triggered,
            Result = batch,
            Memory = new()
            {
                LastPollingTime = DateTime.UtcNow,
                Triggered = triggered
            }
        };
    }
}