﻿using Blackbird.Applications.Sdk.Common;

namespace Apps.AzureOpenAI.Models.Dto;

public class UsageDto
{
    [Display("Prompt tokens")] public int PromptTokens { get; set; }

    [Display("Completion tokens")] public int CompletionTokens { get; set; }

    [Display("Total tokens")] public int TotalTokens { get; set; }
    
    public static UsageDto operator +(UsageDto u1, UsageDto u2)
    {
        return new UsageDto
        {
            PromptTokens = u1.PromptTokens + u2.PromptTokens,
            TotalTokens = u1.TotalTokens + u2.TotalTokens,
            CompletionTokens = u1.CompletionTokens + u2.CompletionTokens
        };
    }

    public UsageDto()
    {
    }

    public UsageDto(CompletionsUsage usageMetadata)
    {
        PromptTokens = usageMetadata.PromptTokens;
        TotalTokens = usageMetadata.TotalTokens;
        CompletionTokens = usageMetadata.CompletionTokens;
    }
    
    public UsageDto(Azure.AI.OpenAI.CompletionsUsage usageMetadata)
    {
        PromptTokens = usageMetadata.PromptTokens;
        TotalTokens = usageMetadata.TotalTokens;
        CompletionTokens = usageMetadata.CompletionTokens;
    }
}
