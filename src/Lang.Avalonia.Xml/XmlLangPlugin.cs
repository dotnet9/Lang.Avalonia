using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Lang.Avalonia.Xml;

public class XmlLangPlugin : ILangPlugin
{
    public Dictionary<string, LocalizationLanguage> Resources { get; } = new();

    public string ResourceFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    private LocalizationLanguage _defaultLanguage = null!;

    public CultureInfo Culture { get; set; } = null!;

    [MemberNotNull(nameof(Culture), nameof(_defaultLanguage))]
    public void Load(CultureInfo cultureInfo)
    {
        Culture = cultureInfo;

        var xmlFiles = Directory.GetFiles(ResourceFolder, "*.xml", SearchOption.AllDirectories);

        foreach (var xmlFile in xmlFiles)
        {
            try
            {
                var xmlDoc = XDocument.Load(xmlFile);

                var root = xmlDoc.Root;

                if (GetAttributeString(root, Consts.LanguageKey) is not { } language
                    || GetAttributeString(root, Consts.DescriptionKey) is not { } description
                    || GetAttributeString(root, Consts.CultureNameKey) is not { } cultureName)
                    continue;

                var localizationLanguage = new LocalizationLanguage
                {
                    Language = language,
                    Description = description,
                    CultureName = cultureName
                };

                Resources.TryAdd(localizationLanguage.CultureName, localizationLanguage);

                if (localizationLanguage.CultureName == cultureInfo.Name)
                    _defaultLanguage = localizationLanguage;

                var propertyNodes = xmlDoc.Nodes().OfType<XElement>().DescendantsAndSelf()
                    .Where(e => !e.HasElements);

                foreach (var propertyNode in propertyNodes)
                {
                    var ancestorsNodeNames = propertyNode.AncestorsAndSelf().Reverse().Select(node => node.Name.LocalName);
                    var key = string.Join('.', ancestorsNodeNames);
                    Resources[localizationLanguage.CultureName].Languages[key] = propertyNode.Value;
                }
            }
            catch
            {
                // ignored
            }
        }

        if (_defaultLanguage is null)
            throw new InvalidDataException("Missing default culture resources");

        return;

        static string? GetAttributeString(XElement? element, string attributeName)
        {
            var value = element?.Attribute(attributeName)?.Value;
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    public void AddResource(params IEnumerable<Assembly> assemblies) => throw new NotSupportedException(nameof(AddResource));

    public IReadOnlyCollection<LocalizationLanguage> GetLanguages() => Resources.Values;

    public string GetResource(string key, string? cultureName = null)
    {
        ((ILangPlugin) this).EnsureLoaded();

        if (string.IsNullOrWhiteSpace(cultureName))
            cultureName = Culture.Name;

        if (((ILangPlugin) this).GetResourceInternal(cultureName, key, out var resource))
            return resource;

        return _defaultLanguage.Languages.GetValueOrDefault(key, key);
    }
}
