﻿using System.Text;
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

    private static List<Blackbird.Xliff.Utils.Models.Tag> GetTags(string src)
    {
        var parsedTags = new List<Blackbird.Xliff.Utils.Models.Tag>();
        var tags = Regex.Matches(src, "<(.*?) (.*?)>(.*?)<\\/\\1>");
        if (tags is null || tags.Count == 0 ) return parsedTags;
        var count = 0;
        foreach ( var tag in tags.Select(x => x.Value)) 
        {
            count++;
            var type = Regex.Match(tag, "<(.*?) ").Groups[1].Value;
            parsedTags.Add(new Blackbird.Xliff.Utils.Models.Tag 
            {
                Position = count,
                Id = Regex.Match(tag.ToLower(), "id=\"(.*?)\"").Groups[1].Value,
                Value = type == "g" || type == "x" ? "" : tag,
                Type = type
            });

        }
        return parsedTags;
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
                var newtu = Regex.Replace(tu, "(<target(.*?)>)([\\s\\S]+?)(</target>)", "${1}" + results[id] +"${4}");
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
            var newTags = GetTags(update.Value);
            if (AreTagsOk(translationUnits.FirstOrDefault(x => x.Id == update.Key).Tags, newTags)) 
            {
                changesToImplement.Add(update.Key, update.Value);
            }
        }

        return changesToImplement;
    }

    private static bool AreTagsOk(List<Blackbird.Xliff.Utils.Models.Tag> tags, List<Blackbird.Xliff.Utils.Models.Tag> newTags)
    {
        if (tags.Count != newTags.Count)
        { return false; }
        foreach (var tag in newTags)
        {
            if (tags.Any(x => x.Id == tag.Id && x.Type == tag.Type && x.Value == tag.Value && x.Position == tag.Position))
            {
                continue;
            }
            else if (tags.Any(x => x.Id == tag.Id && x.Type == tag.Type && x.Value == tag.Value)) 
            {
                if (tag.Type == "bpt" || tag.Type == "ept")
                {
                    if (tag.Type == "bpt") 
                    {

                        if (newTags.Any(x => x.Type == "ept" && x.Id == tag.Id && x.Position > tag.Position)) continue;
                        else return false;
                    } 
                    else if (tag.Type == "ept") 
                    {
                        if (newTags.Any(x => x.Type == "bpt" && x.Id == tag.Id && x.Position < tag.Position)) continue;
                        else return false;
                    }
                }
                else continue;
            }
            else return false;
        }
        return true;
    }
}

