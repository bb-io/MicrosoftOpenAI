using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.PostEdit;

public record BatchProcessingOptions(
    string ApiVersion,
    string SourceLanguage,
    string TargetLanguage,
    string? Prompt,
    string? SystemPrompt,
    bool OverwritePrompts,
    FileReference? Glossary,
    bool FilterGlossary,
    int MaxRetryAttempts,
    int? MaxTokens,
    float? Temperature,
    float? TopP,
    float? FrequencyPenalty,
    float? PresencePenalty,
    string? ReasoningEffort);