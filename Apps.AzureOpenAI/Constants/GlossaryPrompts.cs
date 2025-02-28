using System.Text;
using System.Text.RegularExpressions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Glossaries.Utils.Dtos;

namespace Apps.AzureOpenAI.Constants;

public static class GlossaryPrompts
{
    public static string? GetGlossaryPromptPart(Glossary blackbirdGlossary, string sourceContentInJson, bool? filter)
    {
        var glossaryPromptPart = new StringBuilder();
        glossaryPromptPart.AppendLine();
        glossaryPromptPart.AppendLine();
        glossaryPromptPart.AppendLine("Glossary entries (each entry includes terms in different language. Each " +
                                      "language may have a few synonymous variations which are separated by ;;):");

        var entriesIncluded = false;
        foreach (var entry in blackbirdGlossary.ConceptEntries)
        {
            if (TryProcessGlossaryEntry(entry, sourceContentInJson, glossaryPromptPart, filter))
            {
                entriesIncluded = true;
            }
        }

        return entriesIncluded ? glossaryPromptPart.ToString() : null;
    }
    
    private static bool TryProcessGlossaryEntry(GlossaryConceptEntry entry, string sourceContentInJson, StringBuilder glossaryPromptPart, bool? filter)
    {
        try
        {
            var allTerms = entry.LanguageSections.SelectMany(x => x.Terms.Select(y => y.Term));
            if (filter.HasValue && filter == true && !allTerms.Any(x =>
                    Regex.IsMatch(sourceContentInJson, $@"\b{Regex.Escape(x)}\b", RegexOptions.IgnoreCase)))
            {
                return false;
            }

            glossaryPromptPart.AppendLine();
            glossaryPromptPart.AppendLine("\tEntry:");

            foreach (var section in entry.LanguageSections)
            {
                glossaryPromptPart.AppendLine(
                    $"\t\t{section.LanguageCode}: {string.Join(";; ", section.Terms.Select(term => term.Term))}");
            }

            return true;
        }
        catch (Exception e)
        {
            throw new PluginApplicationException(
                $"The action failed while trying to insert a glossary entry into a request. The failure occurred on the following entry ID: {entry.Id}. " +
                $"Error message: {e.Message}. Please check your glossary. If no issues are found, please contact support.");
        }
    }

    public static string GetGlossaryWithUserPrompt(string userPrompt, string? glossaryPromptPart)
    {
        if (glossaryPromptPart != null)
        {
            var glossaryPrompt =
                "Enhance the target text by incorporating relevant terms from our glossary where applicable. " +
                "Ensure that the translation aligns with the glossary entries for the respective languages. " +
                "If a term has variations or synonyms, consider them and choose the most appropriate " +
                "translation to maintain consistency and precision. ";
            glossaryPrompt += glossaryPromptPart;
            userPrompt += glossaryPrompt;
        }

        return userPrompt;
    }
}