namespace Apps.AzureOpenAI.Constants;

public static class UserPrompts
{
    private const string TranslatePrompt = "$Translate the following texts from {source_language} to {target_language}";

    private const string ProcessPrompt =
        "Process the following texts as per the custom instructions: {prompt}. The source language is {source_language} and the target language is {target_language}. This information might be useful for the custom instructions.";

    private const string SummarizePrompt =
        "Please provide a translation for each individual text, even if similar texts have been provided more than once. " +
        "{instruction} Return the outputs as a serialized JSON array of strings without additional formatting " +
        "(it is crucial because your response will be deserialized programmatically. Please ensure that your response is formatted correctly to avoid any deserialization issues). " +
        "Original texts (in serialized array format): {json};\nReply with the processed text preserving the same format structure as provided, your output will need to be deserialized programmatically afterwards. Do not add linebreaks.";

    private const string PostEditPrompt =
        "Your input consists of sentences in {source_language} language with their translations into {target_language}. " +
        "Review and edit the translated target text as necessary to ensure it is a correct and accurate translation of the source text. " +
        "If you see XML tags in the source also include them in the target text, don't delete or modify them. " +
        "{prompt}; {glossary_prompt}. " +
        "Translation units: {json}.";

    public const string SystemPrompt =
        "You are a linguistic expert that should process the following texts according to the given instructions";

    private static string GetTranslatePrompt(string sourceLanguage, string targetLanguage)
    {
        return TranslatePrompt.Replace("{source_language}", sourceLanguage)
            .Replace("{target_language}", targetLanguage);
    }

    private static string GetProcessPrompt(string prompt, string sourceLanguage, string targetLanguage)
    {
        return ProcessPrompt.Replace("{prompt}", prompt).Replace("{source_language}", sourceLanguage)
            .Replace("{target_language}", targetLanguage);
    }

    public static string GetProcessPrompt(string? userInstructions, string sourceLanguage, string targetLanguage,
        string json)
    {
        var instructions = string.IsNullOrEmpty(userInstructions)
            ? GetTranslatePrompt(sourceLanguage, targetLanguage)
            : GetProcessPrompt(userInstructions, sourceLanguage, targetLanguage);

        return SummarizePrompt.Replace("{instruction}", instructions).Replace("{json}", json);
    }

    public static string GetPostEditPrompt(string? prompt, string? glossaryPrompt, string sourceLanguage,
        string targetLanguage, string json)
    {
        var result = prompt == null 
            ? PostEditPrompt.Replace("{prompt}; ", string.Empty) 
            : PostEditPrompt.Replace("{prompt}", prompt);
        
        result = glossaryPrompt == null 
            ? result.Replace("{glossary_prompt}. ", string.Empty) 
            : result.Replace("{glossary_prompt}", glossaryPrompt);
        
        return result.Replace("{source_language}", sourceLanguage)
            .Replace("{target_language}", targetLanguage)
            .Replace("{json}", json);
    }
}