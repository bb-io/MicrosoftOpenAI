using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.AzureOpenAI.Models.Requests.Chat
{
    public class BaseChatRequest
    {
        [Display("Maximum tokens")]
        public int? MaximumTokens { get; set; }

        [Display("Temperature")]
        [DataSource(typeof(TemperatureDataSourceHandler))]
        public float? Temperature { get; set; }

        [Display("Presence penalty")]
        [DataSource(typeof(PenaltyDataSourceHandler))]
        public float? PresencePenalty { get; set; }

        [Display("Frequency penalty")]
        [DataSource(typeof(PenaltyDataSourceHandler))]
        public float? FrequencyPenalty { get; set; }
    }
}
