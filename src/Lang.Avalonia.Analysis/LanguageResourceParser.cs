using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace Lang.Avalonia.Analysis;

public static class LanguageResourceParser
{
    public static Dictionary<string, Dictionary<string, string>> ParseJsonFile(string filePath, string content)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        
        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (GetPropertyString(root, Consts.LanguageKey) is null
                || GetPropertyString(root, Consts.DescriptionKey) is null
                || GetPropertyString(root, Consts.CultureNameKey) is not { } cultureName)
                return result;

            var allProperties = new Dictionary<string, string>();
            CollectJsonProperties(root, "", allProperties);

            result[cultureName] = allProperties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        catch
        {
            // 忽略解析错误
        }

        return result;

        static string? GetPropertyString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var languageProp))
                return null;
            var value = languageProp.GetString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    public static Dictionary<string, Dictionary<string, string>> ParseXmlFile(string filePath, string content)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        
        try
        {
            var doc = XDocument.Parse(content);
            var root = doc.Root;

            if (GetAttributeString(root, Consts.LanguageKey) is null
                || GetAttributeString(root, Consts.DescriptionKey) is null
                || GetAttributeString(root, Consts.CultureNameKey) is not { } cultureName)
                return result;

            var properties = new Dictionary<string, string>();
            var propertyNodes = doc.Nodes().OfType<XElement>().DescendantsAndSelf()
                .Where(e => !e.HasElements);
                
            foreach (var propertyNode in propertyNodes)
            {
                var ancestorsNodeNames = propertyNode.AncestorsAndSelf().Reverse().Select(node => node.Name.LocalName);
                var key = string.Join(".", ancestorsNodeNames);
                properties[key] = propertyNode.Value;
            }

            result[cultureName] = properties;
        }
        catch
        {
            // 忽略解析错误
        }

        return result;

        static string? GetAttributeString(XElement? element, string attributeName)
        {
            var value = element?.Attribute(attributeName)?.Value;
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    public static Dictionary<string, Dictionary<string, string>> ParseResxFile(string filePath, string content)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        
        try
        {
            var doc = XDocument.Parse(content);
            var properties = new Dictionary<string, string>();

            var dataElements = doc.Descendants("data");
            foreach (var dataElement in dataElements)
            {
                var name = dataElement.Attribute("name")?.Value;
                var valueElement = dataElement.Element("value");
                
                if (!string.IsNullOrEmpty(name) && valueElement != null)
                {
                    properties[name!] = valueElement.Value ?? string.Empty;
                }
            }

            // Resx文件没有内置的culture信息，使用文件名推断
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var cultureName = ExtractCultureFromFileName(fileName);
            
            result[cultureName] = properties;
        }
        catch
        {
            // 忽略解析错误
        }

        return result;
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
                var index = 0;
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

                // 过滤掉根节点的元数据属性
                if (Consts.LanguageKey.Equals(currentPath, StringComparison.OrdinalIgnoreCase)
                    || Consts.DescriptionKey.Equals(currentPath, StringComparison.OrdinalIgnoreCase)
                    || Consts.CultureNameKey.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
                    return;

                result[currentPath] = element.ToString();
                break;
        }
    }

    private static string ExtractCultureFromFileName(string fileName)
    {
        // 处理类似 "Resources.zh-CN" 的文件名
        var parts = fileName.Split('.');
        if (parts.Length > 1)
        {
            var lastPart = parts.Last();
            if (lastPart.Contains("-") && lastPart.Length >= 2)
            {
                return lastPart;
            }
        }

        return "en-US"; // 默认文化
    }

    public static LanguageFileType DetectFileType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".json" => LanguageFileType.Json,
            ".xml" => LanguageFileType.Xml,
            ".resx" => LanguageFileType.Resx,
            _ => LanguageFileType.Unknown
        };
    }
}

public enum LanguageFileType
{
    Unknown,
    Json,
    Xml,
    Resx
}

static file class Consts
{
    public const string LanguageKey = "language";

    public const string DescriptionKey = "description";

    public const string CultureNameKey = "cultureName";
}
