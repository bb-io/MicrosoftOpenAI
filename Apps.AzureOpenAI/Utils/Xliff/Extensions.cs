using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Blackbird.Xliff.Utils.Models;

namespace Apps.AzureOpenAI.Utils.Xliff;

public static class Extensions 
{
    public static Stream ToStream(this XDocument xDoc) 
    {
        var stringWriter = new StringWriter();
        xDoc.Save(stringWriter);
        var encoding = stringWriter.Encoding;

        var content = stringWriter.ToString();
        content = content.Replace("&lt;", "<").Replace("&gt;", ">");
        content = content.Replace(" />", "/>");

        var memoryStream = new MemoryStream(encoding.GetBytes(content));
        memoryStream.Position = 0;

        return memoryStream;
    }

    public static string RemoveExtraNewLines(string originalString) 
    {
        if (!string.IsNullOrWhiteSpace(originalString)) 
        {
            var to_modify = originalString;
            to_modify = Regex.Replace(to_modify, @"\r\n(\s+)?", "", RegexOptions.Multiline);
            return to_modify;
        }
        else 
        {
            return string.Empty;
        }
    }

    public static Stream UpdateOriginalFile(Stream fileStream, Dictionary<string, string> results)
    {
        string fileContent;
        Encoding encoding;

        using (StreamReader inFileStream = new StreamReader(fileStream)) 
        {
            encoding = inFileStream.CurrentEncoding;
            fileContent = inFileStream.ReadToEnd();
        }

        var tus = Regex.Matches(fileContent, @"<trans-unit [\s\S]+?</trans-unit>").Select(x => x.Value);
        foreach (var tu in tus) 
        {
            var id = Regex.Match(tu, @"trans-unit id=""(.*?)""").Groups[1].Value;
            if (results.ContainsKey(id)) 
            {
                var newtu = Regex.Replace(tu, "(<target(.*?)>)([\\s\\S]+?)(</target>)", "${1}" + results[id] + "${4}");
                fileContent = Regex.Replace(fileContent, Regex.Escape(tu), newtu);

            }
            else continue;
        }
        return new MemoryStream(encoding.GetBytes(fileContent));
    }

    public static Dictionary<string, string> CheckTagIssues(List<TranslationUnit> translationUnits, Dictionary<string, string> results) 
    {
        var changesToImplement = new Dictionary<string, string>();
        foreach (var update in results) 
        {
            var translationUnit = translationUnits.FirstOrDefault(x => x.Id == update.Key);
            if (translationUnit == null) 
            {
                continue;
            }

            var newTags = GetTags(update.Value);
            if (AreTagsOk(translationUnit.Tags, newTags)) 
            {
                changesToImplement.Add(update.Key, update.Value);
            }
            else 
            {
                changesToImplement.Add(update.Key, update.Value);
            }
        }

        return changesToImplement;
    }

    private static List<Blackbird.Xliff.Utils.Models.Tag> GetTags(string src) 
    {
        var parsedTags = new List<Blackbird.Xliff.Utils.Models.Tag>();
        var count = 0;

        var tags = Regex.Matches(src, "<(g|x)[^>]*?(?:/>|>.*?</\\1>)");

        foreach (Match match in tags) 
        {
            count++;
            var fullTag = match.Value;
            var type = Regex.Match(fullTag, @"^<\s*([a-zA-Z0-9]+)").Groups[1].Value;
            var id = Regex.Match(fullTag, @"id\s*=\s*""([^""]+)""").Groups[1].Value;

            parsedTags.Add(new Blackbird.Xliff.Utils.Models.Tag 
            {
                Position = count,
                Id = id,
                Value = type == "g" || type == "x" ? "" : fullTag,
                Type = type
            });
        }

        return parsedTags;
    }

    private static bool AreTagsOk(List<Blackbird.Xliff.Utils.Models.Tag> expected, List<Blackbird.Xliff.Utils.Models.Tag> actual) 
    {
        var expectedIds = expected.Select(t => t.Id).ToHashSet();
        var actualIds = actual.Select(t => t.Id).ToHashSet();

        return expectedIds.IsSubsetOf(actualIds);
    }
}

