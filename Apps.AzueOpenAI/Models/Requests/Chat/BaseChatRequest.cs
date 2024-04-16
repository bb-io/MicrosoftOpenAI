using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AzureOpenAI.Models.Requests.Chat
{
    public class BaseChatRequest
    {
        [Display("Maximum tokens")]
        public int? MaximumTokens { get; set; }

        [Display("Temperature")]
        [StaticDataSource(typeof(TemperatureDataSourceHandler))]
        public float? Temperature { get; set; }

        [Display("Presence penalty")]
        [StaticDataSource(typeof(PenaltyDataSourceHandler))]
        public float? PresencePenalty { get; set; }

        [Display("Frequency penalty")]
        [StaticDataSource(typeof(PenaltyDataSourceHandler))]
        public float? FrequencyPenalty { get; set; }
    }
}
