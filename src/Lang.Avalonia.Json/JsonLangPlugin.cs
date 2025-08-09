using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Lang.Avalonia.Json;

public class JsonLangPlugin : ILangPlugin
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Dictionary<string, LocalizationLanguage> Resources { get; private set; }= new();
    public string ResourceFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
    public CultureInfo Culture { get; set; }

    public void Load(CultureInfo cultureInfo)
    {
        Culture = cultureInfo;

        // 获取所有JSON文件并筛选有效语言文件
        var jsonFiles = Directory.GetFiles(ResourceFolder, "*.json", SearchOption.AllDirectories)
            .Where(IsValidLanguageFile)
            .ToList();

        if (!jsonFiles.Any())
        {
            Console.WriteLine("Please provide valid language JSON files");
            return;
        }

        foreach (var jsonFile in jsonFiles)
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(jsonFile));
            var root = doc.RootElement;

            // 解析根节点的元数据
            var language = new LocalizationLanguage
            {
                Language = root.GetProperty("language").GetString()!,
                Description = root.GetProperty("description").GetString()!,
                CultureName = root.GetProperty("cultureName").GetString()!,
            };

            Resources.TryAdd(language.CultureName, language);

            // 递归收集所有键值对（排除根节点的三个元数据属性）
            var allProperties = new Dictionary<string, string>();
            CollectJsonProperties(root, "", allProperties);

            // 过滤掉根节点的元数据属性
            var excludeKeys = new[] { "language", "description", "cultureName" };
            foreach (var (key, value) in allProperties)
            {
                if (!excludeKeys.Any(k => key.Equals(k, StringComparison.OrdinalIgnoreCase)))
                {
                    Resources[language.CultureName].Languages[key] = value;
                }
            }
        }
    }

    // 验证JSON文件是否包含必要的根属性
    private bool IsValidLanguageFile(string filePath)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(filePath));
            var root = doc.RootElement;

            return root.TryGetProperty("language", out _)
                && root.TryGetProperty("description", out _)
                && root.TryGetProperty("cultureName", out _);
        }
        catch
        {
            return false;
        }
    }

    // 递归遍历JSON元素，收集所有键值对（生成类似XML的层级键）
    private void CollectJsonProperties(JsonElement element, string currentPath, Dictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                // 遍历对象的所有属性
                foreach (var property in element.EnumerateObject())
                {
                    var newPath = string.IsNullOrEmpty(currentPath)
                        ? property.Name
                        : $"{currentPath}.{property.Name}";

                    CollectJsonProperties(property.Value, newPath, result);
                }
                break;

            case JsonValueKind.Array:
                // 处理数组（按索引拼接键名）
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var newPath = $"{currentPath}[{index}]";
                    CollectJsonProperties(item, newPath, result);
                    index++;
                }
                break;

            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                // 处理基本类型值
                result[currentPath] = element.ToString();
                break;
        }
    }

    public List<LocalizationLanguage>? GetLanguages() => Resources?.Select(kvp => kvp.Value).ToList();

    public string? GetResource(string key, string? cultureName = null)
    {
        var culture = Culture?.Name ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            culture = cultureName;
        }

        if (Resources?.TryGetValue(culture, out var currentLanguages) == true
            && currentLanguages.Languages.TryGetValue(key, out string resource))
        {
            return resource;
        }

        return key;
    }
}
