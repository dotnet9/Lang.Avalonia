using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Lang.Avalonia.Analysis;

internal record struct LanguageFileInfo(string Path, string Content);

/// <summary>
/// 根据 AdditionalFiles 中的语言资源生成强类型资源 Key 常量。
/// </summary>
[Generator]
public class LanguageSourceGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor InvalidResourceFile = new(
        "LAA002",
        "Invalid language resource",
        "No valid language resource entries were found in '{0}'",
        "Lang.Avalonia.Analysis",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidResourceKey = new(
        "LAA003",
        "Unsupported language resource key",
        "Language resource key '{0}' must contain at least three dot-separated segments",
        "Lang.Avalonia.Analysis",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var additionalFiles = context.AdditionalTextsProvider
            .Where(static file => IsLanguageFile(file.Path))
            .Select(static (file, cancellationToken) => new LanguageFileInfo
            (
                file.Path,
                file.GetText(cancellationToken)?.ToString() ?? string.Empty
            ))
            .Collect();

        context.RegisterSourceOutput(additionalFiles, static (context, files) => GenerateLanguageSource(context, files));
    }

    private static bool IsLanguageFile(string filePath)
    {
        return LanguageResourceParser.DetectFileType(filePath) != LanguageFileType.Unknown;
    }

    private static void GenerateLanguageSource(SourceProductionContext context, ImmutableArray<LanguageFileInfo> files)
    {
        try
        {
            var allResources = new Dictionary<string, Dictionary<string, string>>();

            foreach (var file in files)
            {
                var filePath = file.Path;
                var content = file.Content;

                if (string.IsNullOrEmpty(content))
                {
                    continue;
                }

                var fileType = LanguageResourceParser.DetectFileType(filePath);

                var fileResources = fileType switch
                {
                    LanguageFileType.Json => LanguageResourceParser.ParseJsonFile(filePath, content),
                    LanguageFileType.Xml => LanguageResourceParser.ParseXmlFile(filePath, content),
                    LanguageFileType.Resx => LanguageResourceParser.ParseResxFile(filePath, content),
                    _ => new Dictionary<string, Dictionary<string, string>>()
                };

                if (fileResources.Count == 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(InvalidResourceFile, Location.None, filePath));
                    continue;
                }

                foreach (var cultureResources in fileResources)
                {
                    var cultureName = cultureResources.Key;
                    var resources = cultureResources.Value;

                    if (!allResources.ContainsKey(cultureName))
                    {
                        allResources[cultureName] = new Dictionary<string, string>();
                    }

                    foreach (var resource in resources)
                    {
                        allResources[cultureName][resource.Key] = resource.Value;
                    }
                }
            }

            if (!allResources.Any())
            {
                return;
            }

            foreach (var key in allResources.Values
                         .SelectMany(resources => resources.Keys)
                         .Distinct(System.StringComparer.Ordinal)
                         .Where(key => key.Split('.').Length < 3))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidResourceKey, Location.None, key));
            }

            var generatedCode = LanguageCodeGenerator.GenerateLanguageConstants(allResources);
            if (!string.IsNullOrEmpty(generatedCode))
            {
                var sourceText = SourceText.From(generatedCode, Encoding.UTF8);
                context.AddSource("Language.g.cs", sourceText);
            }
        }
        catch (System.Exception ex)
        {
            var descriptor = new DiagnosticDescriptor(
                "LAA001",
                "Language Analysis Error",
                "Error occurred during language analysis: {0}",
                "Lang.Avalonia.Analysis",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            var diagnostic = Diagnostic.Create(descriptor, Location.None, ex.Message);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
