﻿using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AzureOpenAI.Models.Requests.Chat;

public class SystemChatRequest : BaseChatRequest
{
    [Display("System prompt")]
    public string SystemPrompt { get; set; }
        
    public string Message { get; set; }

}