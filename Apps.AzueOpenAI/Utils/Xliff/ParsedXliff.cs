namespace Apps.AzureOpenAI.Utils.Xliff;

public class ParsedXliff
{
    public string SourceLanguage { get; set; }
    public string TargetLanguage { get; set; }

    public List<TransUnit> TranslationUnits { get; set; }
}