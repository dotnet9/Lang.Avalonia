using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Lang.Avalonia.Json;

/// <summary>
/// JSON 语言资源插件。支持从输出目录扫描 JSON 文件，也支持从额外程序集中读取嵌入资源。
/// </summary>
public class JsonLangPlugin : ILangPlugin
{
    private CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
    private readonly List<string> _loadDiagnostics = new();

    /// <summary>
    /// 已加载的语言资源缓存，Key 为文化名称。
    /// </summary>
    public Dictionary<string, LocalizationLanguage> Resources { get; } = new();

    /// <summary>
    /// JSON 文件扫描目录，默认使用应用程序输出目录。
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

        foreach (var jsonFile in Directory.GetFiles(ResourceFolder, "*.json", SearchOption.AllDirectories))
        {
            TryAddLanguageFile(jsonFile);
        }

        if (Resources.Count == 0)
        {
            _loadDiagnostics.Add("Please provide valid language JSON files.");
        }
    }

    /// <inheritdoc />
    public void AddResource(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies.Where(assembly => assembly != null).Distinct())
        {
            foreach (var resourceName in assembly.GetManifestResourceNames()
                         .Where(name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
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
            using var doc = JsonDocument.Parse(File.ReadAllText(filePath));
            return TryAddLanguage(doc.RootElement);
        }
        catch
        {
            _loadDiagnostics.Add($"Invalid language JSON file skipped: {filePath}");
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

            using var doc = JsonDocument.Parse(stream);
            return TryAddLanguage(doc.RootElement);
        }
        catch
        {
            _loadDiagnostics.Add($"Invalid embedded language JSON resource skipped: {assembly.GetName().Name}/{resourceName}");
            return false;
        }
    }

    private bool TryAddLanguage(JsonElement root)
    {
        if (!TryReadLanguage(root, out var language))
        {
            return false;
        }

        if (!Resources.TryGetValue(language.CultureName, out var currentLanguage))
        {
            currentLanguage = language;
            Resources[language.CultureName] = currentLanguage;
        }

        var allProperties = new Dictionary<string, string>();
        CollectJsonProperties(root, string.Empty, allProperties);

        var excludeKeys = new[] { Consts.LanguageKey, Consts.DescriptionKey, Consts.CultureNameKey };
        foreach (var (key, value) in allProperties)
        {
            if (!excludeKeys.Any(k => key.Equals(k, StringComparison.OrdinalIgnoreCase)))
            {
                currentLanguage.Languages[key] = value;
            }
        }

        return true;
    }

    private static bool TryReadLanguage(JsonElement root, out LocalizationLanguage language)
    {
        language = new LocalizationLanguage();

        if (!root.TryGetProperty(Consts.LanguageKey, out var languageElement)
            || !root.TryGetProperty(Consts.DescriptionKey, out var descriptionElement)
            || !root.TryGetProperty(Consts.CultureNameKey, out var cultureNameElement))
        {
            return false;
        }

        var languageName = languageElement.GetString();
        var description = descriptionElement.GetString();
        var cultureName = cultureNameElement.GetString();
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

    private static void CollectJsonProperties(JsonElement element, string currentPath, Dictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var newPath = string.IsNullOrEmpty(currentPath)
                        ? property.Name
                        : $"{currentPath}.{property.Name}";

                    CollectJsonProperties(property.Value, newPath, result);
                }

                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    CollectJsonProperties(item, $"{currentPath}[{index}]", result);
                    index++;
                }

                break;

            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                result[currentPath] = element.ToString();
                break;
        }
    }
}
