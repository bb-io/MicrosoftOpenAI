using Apps.AzureOpenAI.DataSourceHandlers;
using Apps.AzureOpenAI.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class BaseChatRequest
{
    [Display("Maximum tokens")]
    public int? MaximumTokens { get; set; }

    [Display("Temperature")]
    [StaticDataSource(typeof(TemperatureDataSourceHandler))]
    public float? Temperature { get; set; }    
    
    [Display("Top p")]
    [StaticDataSource(typeof(TopPDataSourceHandler))]
    public float? TopP { get; set; }

    [Display("Presence penalty")]
    [StaticDataSource(typeof(PenaltyDataSourceHandler))]
    public float? PresencePenalty { get; set; }

    [Display("Frequency penalty")]
    [StaticDataSource(typeof(PenaltyDataSourceHandler))]
    public float? FrequencyPenalty { get; set; }
    
    [Display("Reasoning effort")]
    [StaticDataSource(typeof(ReasoningEffortDataSourceHandler))]
    public string? ReasoningEffort { get; set; }
}
