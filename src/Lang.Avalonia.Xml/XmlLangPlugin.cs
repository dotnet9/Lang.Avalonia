using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Lang.Avalonia.Xml;

/// <summary>
/// XML 语言资源插件。支持从输出目录扫描 XML 文件，也支持从额外程序集中读取嵌入资源。
/// </summary>
public class XmlLangPlugin : ILangPlugin
{
    private CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
    private readonly List<string> _loadDiagnostics = new();

    /// <summary>
    /// 已加载的语言资源缓存，Key 为文化名称。
    /// </summary>
    public Dictionary<string, LocalizationLanguage> Resources { get; } = new();

    /// <summary>
    /// XML 文件扫描目录，默认使用应用程序输出目录。
    /// </summary>
    public string ResourceFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// 最近一次加载资源时产生的诊断信息。
    /// </summary>
    public IReadOnlyList<string> LoadDiagnostics => _loadDiagnostics;

    /// <inheritdoc />
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    /// <inheritdoc />
    public void Load(CultureInfo cultureInfo)
    {
        _defaultCulture = cultureInfo;
        Culture = cultureInfo;
        Resources.Clear();
        _loadDiagnostics.Clear();

        if (!Directory.Exists(ResourceFolder))
        {
            _loadDiagnostics.Add($"Language resource folder not found: {ResourceFolder}");
            return;
        }

        foreach (var xmlFile in Directory.GetFiles(ResourceFolder, "*.xml", SearchOption.AllDirectories))
        {
            TryAddLanguageFile(xmlFile);
        }

        if (Resources.Count == 0)
        {
            _loadDiagnostics.Add("Please provide valid language XML files.");
        }
    }

    /// <inheritdoc />
    public void AddResource(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies.Where(assembly => assembly != null).Distinct())
        {
            foreach (var resourceName in assembly.GetManifestResourceNames()
                         .Where(name => name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
            {
                TryAddLanguageResource(assembly, resourceName);
            }
        }
    }

    /// <inheritdoc />
    public List<LocalizationLanguage>? GetLanguages() => Resources.Select(kvp => kvp.Value).ToList();

    /// <inheritdoc />
    public string GetResource(string key, string? cultureName = null)
    {
        var culture = Culture.Name;
        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            culture = cultureName;
        }

        if (Resources.TryGetValue(culture, out var currentLanguages)
            && currentLanguages.Languages.TryGetValue(key, out var resource))
        {
            return resource;
        }

        if (Resources.TryGetValue(_defaultCulture.Name, out currentLanguages)
            && currentLanguages.Languages.TryGetValue(key, out resource))
        {
            return resource;
        }

        return key;
    }

    private bool TryAddLanguageFile(string filePath)
    {
        try
        {
            return TryAddLanguage(XDocument.Load(filePath));
        }
        catch
        {
            _loadDiagnostics.Add($"Invalid language XML file skipped: {filePath}");
            return false;
        }
    }

    private bool TryAddLanguageResource(Assembly assembly, string resourceName)
    {
        try
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return false;
            }

            return TryAddLanguage(XDocument.Load(stream));
        }
        catch
        {
            _loadDiagnostics.Add($"Invalid embedded language XML resource skipped: {assembly.GetName().Name}/{resourceName}");
            return false;
        }
    }

    private bool TryAddLanguage(XDocument xmlDoc)
    {
        if (!TryReadLanguage(xmlDoc.Root, out var language))
        {
            return false;
        }

        if (!Resources.TryGetValue(language.CultureName, out var currentLanguage))
        {
            currentLanguage = language;
            Resources[language.CultureName] = currentLanguage;
        }

        var propertyNodes = xmlDoc.Nodes().OfType<XElement>().DescendantsAndSelf()
            .Where(e => e.Descendants().Any() != true);

        foreach (var propertyNode in propertyNodes)
        {
            var ancestorsNodeNames = propertyNode.AncestorsAndSelf().Reverse().Select(node => node.Name.LocalName);
            var key = string.Join(".", ancestorsNodeNames);
            currentLanguage.Languages[key] = propertyNode.Value;
        }

        return true;
    }

    private static bool TryReadLanguage(XElement? element, out LocalizationLanguage language)
    {
        language = new LocalizationLanguage();
        if (element == null)
        {
            return false;
        }

        var languageName = element.Attribute(Consts.LanguageKey)?.Value;
        var description = element.Attribute(Consts.DescriptionKey)?.Value;
        var cultureName = element.Attribute(Consts.CultureNameKey)?.Value;
        if (string.IsNullOrWhiteSpace(languageName)
            || string.IsNullOrWhiteSpace(description)
            || string.IsNullOrWhiteSpace(cultureName))
        {
            return false;
        }

        language = new LocalizationLanguage
        {
            Language = languageName,
            Description = description,
            CultureName = cultureName
        };
        return true;
    }
}
