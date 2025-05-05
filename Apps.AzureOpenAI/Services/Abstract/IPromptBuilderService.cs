using Blackbird.Xliff.Utils.Models;

namespace Apps.AzureOpenAI.Services.Abstract;

public interface IPromptBuilderService
{
    string GetSystemPrompt();
    
    string BuildUserPrompt(
        string sourceLanguage,
        string targetLanguage,
        TranslationUnit[] batch,
        string? additionalPrompt,
        string? glossaryPrompt);
}
