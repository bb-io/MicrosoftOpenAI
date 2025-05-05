using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Xliff.Utils;
using Blackbird.Xliff.Utils.Models;

namespace Apps.AzureOpenAI.Services.Abstract;

public interface IXliffService
{
    Task<XliffDocument> LoadXliffDocumentAsync(FileReference file);
    Stream SerializeXliffDocument(XliffDocument document);
    IEnumerable<IEnumerable<TranslationUnit>> BatchTranslationUnits(
        IEnumerable<TranslationUnit> units, int batchSize);
    Dictionary<string, string> CheckAndFixTagIssues(
        IEnumerable<TranslationUnit> units, Dictionary<string, string> translations, bool disableTagChecks);
}