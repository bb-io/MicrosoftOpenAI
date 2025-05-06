using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Xliff.Utils.Models;

namespace Apps.AzureOpenAI.Services.Abstract;

public interface IGlossaryService
{
    Task<string?> BuildGlossaryPromptAsync(FileReference? glossary, IEnumerable<TranslationUnit> translationUnits, bool filter);
}