using System.Net;
using Apps.AzureOpenAI.Models.Dto;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.AzureOpenAI.Api;

public class AzureOpenAIRestClient(IEnumerable<AuthenticationCredentialsProvider> credentials) : BlackBirdRestClient(
    new()
    {
        ThrowOnAnyError = false,
        BaseUrl = new Uri(credentials.Get("url").Value),
        MaxTimeout = (int)TimeSpan.FromMinutes(15).TotalMilliseconds
    })
{
    protected override Exception ConfigureErrorException(RestResponse response)
    {
        if (response.Content == null)
            throw new PluginApplicationException(response?.ErrorMessage);

        var error = JsonConvert.DeserializeObject<ErrorDtoWrapper>(response.Content, JsonSettings);

        if (response.StatusCode == HttpStatusCode.NotFound && error.Error.Type == "invalid_request_error")
            return new PluginMisconfigurationException("Model chosen is not suitable for this task. Please choose a compatible model.");
        
        return new PluginApplicationException(error?.Error?.Message ?? response.ErrorException!.Message);
    }
}