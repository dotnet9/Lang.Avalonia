using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Lang.Avalonia.Xml;

public class XmlLangPlugin : ILangPlugin
{
    public string ResourcePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    public Dictionary<string, LocalizationLanguage> LoadLanguages()
    {
        var resources = new Dictionary<string, LocalizationLanguage>();

        LocalizationLanguage ReadLanguage(XElement element)
        {
            return new LocalizationLanguage
            {
                Language = element!.Attribute(Consts.LanguageKey)!.Value,
                Description = element.Attribute("description")!.Value,
                CultureName = element.Attribute("cultureName")!.Value
            };
        }

        var xmlFiles = Directory.GetFiles(ResourcePath, "*.xml", SearchOption.AllDirectories)
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
            return resources;
        }

        foreach (var xmlFile in xmlFiles)
        {
            var xmlDoc = XDocument.Load(xmlFile);

            var language = ReadLanguage(xmlDoc.Root!);
            if (!resources.ContainsKey(language.CultureName))
            {
                resources[language.CultureName] = language;
            }

            var propertyNodes = xmlDoc.Nodes().OfType<XElement>().DescendantsAndSelf()
                .Where(e => e.Descendants().Any() != true).ToList();
            foreach (var propertyNode in propertyNodes)
            {
                var ancestorsNodeNames = propertyNode.AncestorsAndSelf().Reverse().Select(node => node.Name.LocalName);
                var key = string.Join(".", ancestorsNodeNames);
                resources[language.CultureName].Languages[key] = propertyNode.Value;
            }
        }

        return resources;
    }
}