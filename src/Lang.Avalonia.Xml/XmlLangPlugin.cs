using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Lang.Avalonia.Xml;

public class XmlLangPlugin : ILangPlugin
{
    public Dictionary<string, LocalizationLanguage> Resources { get;  } = new();
    public string ResourceFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory;


    public CultureInfo Culture { get; set; }

    public void Load(CultureInfo cultureInfo)
    {
        Culture = cultureInfo;

        LocalizationLanguage ReadLanguage(XElement element)
        {
            return new LocalizationLanguage
            {
                Language = element!.Attribute(Consts.LanguageKey)!.Value,
                Description = element.Attribute("description")!.Value,
                CultureName = element.Attribute("cultureName")!.Value
            };
        }

        var xmlFiles = Directory.GetFiles(ResourceFolder, "*.xml", SearchOption.AllDirectories)
            .Where(file =>
            {
                var doc = XDocument.Load(file);
                var root = doc.Root;
                var language = root?.Attribute(Consts.LanguageKey)?.Value;
                var description = root?.Attribute(Consts.DescriptionKey)?.Value;
                var cultureName = root?.Attribute(Consts.CultureNameKey)?.Value;
                return !string.IsNullOrWhiteSpace(language)
                       && !string.IsNullOrWhiteSpace(description)
                       && !string.IsNullOrWhiteSpace(cultureName);
            }).ToList();

        if (xmlFiles.Any() != true)
        {
            Console.WriteLine("Please provide the language XML file");
            return;
        }

        foreach (var xmlFile in xmlFiles)
        {
            var xmlDoc = XDocument.Load(xmlFile);

            var language = ReadLanguage(xmlDoc.Root!);
            if (!Resources.ContainsKey(language.CultureName))
            {
                Resources[language.CultureName] = language;
            }

            var propertyNodes = xmlDoc.Nodes().OfType<XElement>().DescendantsAndSelf()
                .Where(e => e.Descendants().Any() != true).ToList();
            foreach (var propertyNode in propertyNodes)
            {
                var ancestorsNodeNames = propertyNode.AncestorsAndSelf().Reverse().Select(node => node.Name.LocalName);
                var key = string.Join(".", ancestorsNodeNames);
                Resources[language.CultureName].Languages[key] = propertyNode.Value;
            }
        }
    }

    public List<LocalizationLanguage>? GetLanguages() => Resources.Select(kvp => kvp.Value).ToList();

    public string? GetResource(string key, string? cultureName = null)
    {
        var culture = Culture.Name;
        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            culture = cultureName;
        }

        if (Resources.TryGetValue(culture, out var currentLanguages)
            && currentLanguages.Languages.TryGetValue(key, out string resource))
        {
            return resource;
        }

        return key;
    }
}