using Lang.Avalonia;
using Lang.Avalonia.Converters;
using Lang.Avalonia.Json;
using Lang.Avalonia.MarkupExtensions;
using Lang.Avalonia.Resx;
using Lang.Avalonia.Xml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Xunit;

namespace Lang.Avalonia.Tests;

public class CultureLookupTests
{
    [Fact]
    public void JsonLookupNormalizesCultureNamesAndFallsBackThroughParents()
    {
        using var folder = new TemporaryFolder();
        WriteJson(folder.Path, "default.json", "en-us", "Default");
        WriteJson(folder.Path, "traditional.json", "zh-Hant", "Traditional");

        var plugin = new JsonLangPlugin { ResourceFolder = folder.Path };
        plugin.Load(new CultureInfo("en-US"));
        plugin.Culture = new CultureInfo("zh-Hant-TW");

        Assert.Equal("Traditional", plugin.GetResource("Localization.Title"));
        Assert.Equal("Default", plugin.GetResource("Localization.Title", "not-a-culture"));
    }

    [Fact]
    public void XmlLookupNormalizesCultureNames()
    {
        using var folder = new TemporaryFolder();
        File.WriteAllText(Path.Combine(folder.Path, "default.xml"), """
            <Localization language="English" description="Default" cultureName="en-US">
              <Localization><Title>Default</Title></Localization>
            </Localization>
            """);
        File.WriteAllText(Path.Combine(folder.Path, "simplified.xml"), """
            <Localization language="Chinese" description="Simplified" cultureName="zh-cn">
              <Localization><Title>Simplified</Title></Localization>
            </Localization>
            """);

        var plugin = new XmlLangPlugin { ResourceFolder = folder.Path };
        plugin.Load(new CultureInfo("en-US"));
        plugin.Culture = new CultureInfo("zh-CN");

        Assert.Equal("Simplified", plugin.GetResource("Localization.Localization.Title"));
    }

    [Fact]
    public void ResxLookupPreservesEmptyValues()
    {
        var plugin = new ResxLangPlugin();
        plugin.Load(new CultureInfo("en-US"));
        plugin.Resources["en-us"] = new LocalizationLanguage
        {
            Language = "English",
            Description = "English",
            CultureName = "en-US"
        };
        plugin.Resources["en-us"].Languages["Empty"] = string.Empty;

        Assert.Equal(string.Empty, plugin.GetResource("Empty"));
    }

    [Fact]
    public void JsonEmbeddedResourcesAddedBeforeLoadArePreserved()
    {
        using var folder = new TemporaryFolder();
        var plugin = new JsonLangPlugin { ResourceFolder = folder.Path };

        plugin.AddResource(typeof(CultureLookupTests).Assembly);
        plugin.Load(new CultureInfo("en-US"));

        Assert.Equal("Embedded title", plugin.GetResource("Embedded.Title"));
    }

    [Fact]
    public void JsonLanguageListReturnsIndependentSnapshots()
    {
        using var folder = new TemporaryFolder();
        WriteJson(folder.Path, "default.json", "en-US", "Original");

        var plugin = new JsonLangPlugin { ResourceFolder = folder.Path };
        plugin.Load(new CultureInfo("en-US"));
        var snapshot = Assert.Single(plugin.GetLanguages()!);
        snapshot.Languages["Localization.Title"] = "Mutated";

        Assert.Equal("Original", plugin.GetResource("Localization.Title"));
    }

    [Fact]
    public void FixedCultureFormatsArgumentsUsingTheSelectedCulture()
    {
        var plugin = new TestPlugin();
        I18nManager.Instance.Register(plugin, new CultureInfo("en-US"), out var error);
        Assert.Null(error);

        var binding = new I18nBinding("Message", "de-DE", new object[] { 1234.5 });
        var converter = new I18nConverter();
        var result = converter.Convert(
            new List<object?> { new CultureInfo("en-US"), "Message" },
            typeof(string),
            binding,
            new CultureInfo("en-US"));

        Assert.Equal("Value: 1.234,50", result);
    }

    private static void WriteJson(string folder, string fileName, string cultureName, string title)
    {
        File.WriteAllText(Path.Combine(folder, fileName), $$"""
            {
              "language": "Test",
              "description": "Test",
              "cultureName": "{{cultureName}}",
              "Localization": { "Title": "{{title}}" }
            }
            """);
    }

    private sealed class TemporaryFolder : IDisposable
    {
        public TemporaryFolder()
        {
            Path = System.IO.Directory.CreateTempSubdirectory("lang-avalonia-tests-").FullName;
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }

    private sealed class TestPlugin : ILangPlugin
    {
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        public void Load(CultureInfo cultureInfo)
        {
            Culture = cultureInfo;
        }

        public void AddResource(params Assembly[] assemblies)
        {
        }

        public List<LocalizationLanguage>? GetLanguages() => [];

        public string GetResource(string key, string? cultureName = null)
        {
            return key == "Message" ? "Value: {0:N2}" : key;
        }
    }
}
