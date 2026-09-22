using Lang.Avalonia.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Xunit;

namespace Lang.Avalonia.Tests;

public class SourceGeneratorTests
{
    [Fact]
    public void GeneratorUsesTheUnionOfKeysAndProducesValidIdentifiers()
    {
        var result = RunGenerator(
            ("en-US.json", """
                {
                  "language": "English",
                  "description": "English",
                  "cultureName": "en-US",
                  "Localization": {
                    "Main": {
                      "123-View": {
                        "Title": "Title"
                      }
                    }
                  }
                }
                """),
            ("zh-CN.json", """
                {
                  "language": "Chinese",
                  "description": "Chinese",
                  "cultureName": "zh-CN",
                  "Localization": {
                    "Main": {
                      "123-View": {
                        "class": "类",
                        "Second": "第二个"
                      }
                    }
                  }
                }
                """));

        var generated = Assert.Single(Assert.Single(result.Results).GeneratedSources).SourceText.ToString();
        Assert.Contains("public static class _123_View", generated);
        Assert.Contains("public static readonly string _class", generated);
        Assert.Contains("public static readonly string Second", generated);
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Id == "LAA003");
    }

    [Fact]
    public void GeneratorReportsInvalidFilesAndKeys()
    {
        var result = RunGenerator(
            ("invalid.json", "{}"),
            ("invalid-culture.json", """
                {
                  "language": "English",
                  "description": "Invalid",
                  "cultureName": "invalid_culture",
                  "Localization": { "Title": "Value" }
                }
                """),
            ("short.json", """
                {
                  "language": "English",
                  "description": "English",
                  "cultureName": "en-US",
                  "Short": "Value"
                }
                """));

        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == "LAA002");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == "LAA003");
    }

    [Fact]
    public void GeneratorOutputIsStableWhenAdditionalFilesAreReordered()
    {
        var first = GenerateText(
            ("a.json", """
                {
                  "language": "English",
                  "description": "English",
                  "cultureName": "en-US",
                  "Localization": { "Main": { "View": { "Foo-Bar": "Dash" } } }
                }
                """),
            ("b.json", """
                {
                  "language": "Chinese",
                  "description": "Chinese",
                  "cultureName": "zh-CN",
                  "Localization": { "Main": { "View": { "Foo_Bar": "Underscore" } } }
                }
                """));

        var reversed = GenerateText(
            ("b.json", """
                {
                  "language": "Chinese",
                  "description": "Chinese",
                  "cultureName": "zh-CN",
                  "Localization": { "Main": { "View": { "Foo_Bar": "Underscore" } } }
                }
                """),
            ("a.json", """
                {
                  "language": "English",
                  "description": "English",
                  "cultureName": "en-US",
                  "Localization": { "Main": { "View": { "Foo-Bar": "Dash" } } }
                }
                """));

        Assert.Equal(first, reversed);
    }

    private static string GenerateText(params (string Path, string Content)[] files)
    {
        var result = RunGenerator(files);
        return Assert.Single(Assert.Single(result.Results).GeneratedSources).SourceText.ToString();
    }

    private static GeneratorDriverRunResult RunGenerator(params (string Path, string Content)[] files)
    {
        var compilation = CSharpCompilation.Create(
            "Lang.Avalonia.Tests.GeneratorInput",
            new[] { CSharpSyntaxTree.ParseText("public static class Marker { }") });
        var additionalTexts = files
            .Select(file => (AdditionalText)new InMemoryAdditionalText(file.Path, file.Content))
            .ToArray();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new LanguageSourceGenerator())
            .AddAdditionalTexts(ImmutableArray.Create(additionalTexts));

        return driver.RunGenerators(compilation).GetRunResult();
    }

    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly SourceText _text;

        public InMemoryAdditionalText(string path, string content)
        {
            Path = path;
            _text = SourceText.From(content, Encoding.UTF8);
        }

        public override string Path { get; }

        public override SourceText GetText(System.Threading.CancellationToken cancellationToken = default) => _text;
    }
}
