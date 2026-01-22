using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Lang.Avalonia.Json;

public class JsonLangPlugin : ILangPlugin
{
    public Dictionary<string, LocalizationLanguage> Resources { get; } = new();

    public string ResourceFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    private LocalizationLanguage _defaultLanguage = null!;

    public CultureInfo Culture { get; set; } = null!;

    [MemberNotNull(nameof(Culture), nameof(_defaultLanguage))]
    public void Load(CultureInfo cultureInfo)
    {
        Culture = cultureInfo;

        // 获取所有JSON文件并筛选有效语言文件
        var jsonFiles = Directory.GetFiles(ResourceFolder, "*.json", SearchOption.AllDirectories);

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(jsonFile));
                var root = doc.RootElement;

                if (GetPropertyString(root, Consts.LanguageKey) is not { } language
                    || GetPropertyString(root, Consts.DescriptionKey) is not { } description
                    || GetPropertyString(root, Consts.CultureNameKey) is not { } cultureName)
                    continue;

                // 解析根节点的元数据
                var localizationLanguage = new LocalizationLanguage
                {
                    Language = language,
                    Description = description,
                    CultureName = cultureName,
                };

                Resources.TryAdd(localizationLanguage.CultureName, localizationLanguage);

                if (localizationLanguage.CultureName == cultureInfo.Name)
                    _defaultLanguage = localizationLanguage;

                // 递归收集所有键值对（排除根节点的三个元数据属性）
                CollectJsonProperties(root, "", localizationLanguage.Languages);
            }
            catch
            {
                // ignored
            }
        }

        if (_defaultLanguage is null)
            throw new InvalidDataException("Missing default culture resources");

        return;

        static string? GetPropertyString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var languageProp))
                return null;
            var value = languageProp.GetString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    public void AddResource(params IEnumerable<Assembly> assemblies) =>
        throw new NotSupportedException(nameof(AddResource));

    /// <summary>
    /// 递归遍历JSON元素，收集所有键值对（生成类似XML的层级键）
    /// </summary>
    /// <param name="element"></param>
    /// <param name="currentPath"></param>
    /// <param name="result"></param>
    private static void CollectJsonProperties(JsonElement element, string currentPath, Dictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            // 遍历对象的所有属性
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var newPath = string.IsNullOrEmpty(currentPath)
                        ? property.Name
                        : $"{currentPath}.{property.Name}";

                    CollectJsonProperties(property.Value, newPath, result);
                }
                break;

            // 处理数组（按索引拼接键名）
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var newPath = $"{currentPath}[{index}]";
                    CollectJsonProperties(item, newPath, result);
                    index++;
                }
                break;

            // 处理基本类型值
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:

                // 过滤掉根节点的元数据属性
                if (Consts.LanguageKey.Equals(currentPath, StringComparison.OrdinalIgnoreCase)
                    || Consts.DescriptionKey.Equals(currentPath, StringComparison.OrdinalIgnoreCase)
                    || Consts.CultureNameKey.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
                    return;

                result[currentPath] = element.ToString();
                break;
        }
    }

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
