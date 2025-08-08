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

            if (!IsValidJsonLanguageFile(root))
                return result;

            var cultureName = root.GetProperty("cultureName").GetString();
            if (string.IsNullOrEmpty(cultureName))
                return result;

            var allProperties = new Dictionary<string, string>();
            CollectJsonProperties(root, "", allProperties);

            var excludeKeys = new[] { "language", "description", "cultureName" };
            var filteredProperties = allProperties
                .Where(kvp => !excludeKeys.Any(k => kvp.Key.Equals(k, StringComparison.OrdinalIgnoreCase)))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (!string.IsNullOrEmpty(cultureName))
                result[cultureName] = filteredProperties;
        }
        catch
        {
            // 忽略解析错误
        }

        return result;
    }

    public static Dictionary<string, Dictionary<string, string>> ParseXmlFile(string filePath, string content)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        
        try
        {
            var doc = XDocument.Parse(content);
            var root = doc.Root;
            
            if (root == null || !IsValidXmlLanguageFile(root))
                return result;

            var cultureName = root.Attribute("cultureName")?.Value;
            if (string.IsNullOrEmpty(cultureName))
                return result;

            var properties = new Dictionary<string, string>();
            var propertyNodes = doc.Nodes().OfType<XElement>().DescendantsAndSelf()
                .Where(e => e.Descendants().Any() != true).ToList();
                
            foreach (var propertyNode in propertyNodes)
            {
                var ancestorsNodeNames = propertyNode.AncestorsAndSelf().Reverse().Select(node => node.Name.LocalName);
                var key = string.Join(".", ancestorsNodeNames);
                properties[key] = propertyNode.Value;
            }

            if (!string.IsNullOrEmpty(cultureName))
                result[cultureName] = properties;
        }
        catch
        {
            // 忽略解析错误
        }

        return result;
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
                    properties[name] = valueElement.Value ?? string.Empty;
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

    private static bool IsValidJsonLanguageFile(JsonElement root)
    {
        return root.TryGetProperty("language", out _) &&
               root.TryGetProperty("description", out _) &&
               root.TryGetProperty("cultureName", out _);
    }

    private static bool IsValidXmlLanguageFile(XElement root)
    {
        return root.Attribute("language") != null &&
               root.Attribute("description") != null &&
               root.Attribute("cultureName") != null;
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