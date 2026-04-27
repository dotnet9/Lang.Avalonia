using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Lang.Avalonia.Json;

public class JsonLangPlugin : ILangPlugin
{
    private CultureInfo _defaultCulture = CultureInfo.InvariantCulture;

    public Dictionary<string, LocalizationLanguage> Resources { get; } = new();

    public string ResourceFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    public void Load(CultureInfo cultureInfo)
    {
        _defaultCulture = cultureInfo;
        Culture = cultureInfo;
        Resources.Clear();

        if (!Directory.Exists(ResourceFolder))
        {
            Console.WriteLine($"Language resource folder not found: {ResourceFolder}");
            return;
        }

        foreach (var jsonFile in Directory.GetFiles(ResourceFolder, "*.json", SearchOption.AllDirectories))
        {
            TryAddLanguageFile(jsonFile);
        }

        if (Resources.Count == 0)
        {
            Console.WriteLine("Please provide valid language JSON files");
        }
    }

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

    public List<LocalizationLanguage>? GetLanguages() => Resources.Select(kvp => kvp.Value).ToList();

    public string? GetResource(string key, string? cultureName = null)
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
