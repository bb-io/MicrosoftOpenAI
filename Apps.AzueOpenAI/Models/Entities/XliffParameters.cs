using Apps.AzureOpenAI.Models.Requests.Chat;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Entities;

public record XliffParameters(string? Prompt, string SystemPrompt, int BucketSize, BaseChatRequest ChatRequest, FileReference? Glossary, bool? filterTerms);