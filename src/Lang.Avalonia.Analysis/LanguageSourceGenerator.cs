using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Lang.Avalonia.Analysis;

public record struct LanguageFileInfo(string Path, string Content);

[Generator]
public class LanguageSourceGenerator : IIncrementalGenerator
{
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
            // 添加调试信息
            var debugDescriptor = new DiagnosticDescriptor(
                "LAA002",
                "Language Analysis Debug",
                "Processing {0} files: {1}",
                "Lang.Avalonia.Analysis",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);
            
            var filesInfo = string.Join(", ", files.Select(f => f.Path));
            context.ReportDiagnostic(Diagnostic.Create(debugDescriptor, Location.None, files.Length, filesInfo));

            var allResources = new Dictionary<string, Dictionary<string, string>>();

            foreach (var (filePath, content) in files)
            {
                if (string.IsNullOrEmpty(content))
                    continue;

                var fileType = LanguageResourceParser.DetectFileType(filePath);
                
                var fileResources = fileType switch
                {
                    LanguageFileType.Json => LanguageResourceParser.ParseJsonFile(filePath, content),
                    LanguageFileType.Xml => LanguageResourceParser.ParseXmlFile(filePath, content),
                    LanguageFileType.Resx => LanguageResourceParser.ParseResxFile(filePath, content),
                    _ => new Dictionary<string, Dictionary<string, string>>()
                };

                foreach (var cultureResources in fileResources)
                {
                    var cultureName = cultureResources.Key;
                    var resources = cultureResources.Value;

                    if (!allResources.ContainsKey(cultureName))
                        allResources[cultureName] = new Dictionary<string, string>();

                    foreach (var resource in resources)
                    {
                        allResources[cultureName][resource.Key] = resource.Value;
                    }
                }
            }

            if (!allResources.Any())
                return;

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
