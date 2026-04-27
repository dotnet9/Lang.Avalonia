# Lang.Avalonia Technical Notes

[简体中文](Lang.Avalonia.md) | English

This document is intended for maintainers and integrators. For quick start, read [README.md](README.md). For architecture diagrams and flow charts, read [docs/design.en-US.md](docs/design.en-US.md).

## Design Goals

Lang.Avalonia separates Avalonia localization into two layers:

1. The `Lang.Avalonia` core library handles XAML markup extensions, binding refresh, formatting, and culture switching.
2. JSON, XML, and RESX plugins normalize different resource sources into `LocalizationLanguage` dictionaries.

This design keeps UI, ViewModels, and business code independent from the resource file format. Consumers register one `ILangPlugin`, then read text through `{c:I18n}` or `I18nManager.Instance.GetResource`.

## Core Types

| Type | Package | Purpose |
| --- | --- | --- |
| `I18nManager` | `Lang.Avalonia` | Global runtime entry point for plugin registration, culture switching, and binding refresh |
| `I18nExtension` | `Lang.Avalonia` | AXAML markup extension entry point: `{c:I18n ...}` |
| `I18nBinding` | `Lang.Avalonia` | MultiBinding that listens to Culture, resource keys, and formatting arguments |
| `I18nConverter` | `Lang.Avalonia` | Resolves resources, applies `string.Format`, and performs final value conversion |
| `ILangPlugin` | `Lang.Avalonia` | Plugin contract that hides JSON, XML, and RESX loading differences |
| `JsonLangPlugin` | `Lang.Avalonia.Json` | Scans JSON files or embedded JSON resources |
| `XmlLangPlugin` | `Lang.Avalonia.Xml` | Scans XML files or embedded XML resources |
| `ResxLangPlugin` | `Lang.Avalonia.Resx` | Discovers `ResourceManager` and synchronizes RESX resources |
| `LanguageSourceGenerator` | `Lang.Avalonia.Analysis` | Generates type-safe resource keys from `AdditionalFiles` |

## Runtime Flow

1. The application calls `I18nManager.Instance.Register(plugin, defaultCulture, out error)` on startup.
2. The plugin runs `Load(defaultCulture)` and builds its resource cache.
3. `I18nManager` synchronizes the current and default thread `CurrentCulture` / `CurrentUICulture`.
4. `{c:I18n}` bindings in AXAML listen to `I18nManager.Culture`.
5. When `Culture` changes, all bindings re-query resources through the plugin.
6. Plugins fall back from explicit culture, current culture, default culture, and finally the original key.

## JSON Resources

JSON files are useful when language files should remain editable or be processed by external tools. Each file must provide three metadata fields:

```json
{
  "language": "English",
  "description": "English resources",
  "cultureName": "en-US",
  "Localization": {
    "Main": {
      "MainView": {
        "Title": "Lang.Avalonia localization workspace"
      }
    }
  }
}
```

The plugin expands leaf nodes into dot-separated keys:

```text
Localization.Main.MainView.Title
```

Project files should copy JSON resources to the output directory:

```xml
<ItemGroup>
  <None Update="I18n\*.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## XML Resources

XML files are suitable when resource hierarchy should be explicit. The root node stores language metadata and leaf nodes store resource values:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Localization language="English" description="English resources" cultureName="en-US">
  <Main>
    <MainView>
      <Title>Lang.Avalonia localization workspace</Title>
    </MainView>
  </Main>
</Localization>
```

XML files also need to be copied to the output directory:

```xml
<ItemGroup>
  <None Update="I18n\*.xml" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## RESX Resources

RESX files use the standard .NET `ResourceManager` mechanism. Resource names should use full keys:

```xml
<data name="Localization.Main.MainView.Title" xml:space="preserve">
  <value>Lang.Avalonia localization workspace</value>
</data>
```

`ResxLangPlugin` scans loaded assemblies for generated resource designer types and reads their `ResourceManager` properties. The default `Mark` is `i18n`; if a project resource name does not include `I18n`, set it explicitly:

```csharp
new ResxLangPlugin { Mark = "Resources" }
```

## Type-Safe Keys

Demo projects provide T4 templates, and `Lang.Avalonia.Analysis` provides a Source Generator. Both generate constants for `x:Static` and avoid hardcoded strings.

Source Generator configuration:

```xml
<ItemGroup>
  <PackageReference Include="Lang.Avalonia.Analysis" Version="*" PrivateAssets="all" />
  <AdditionalFiles Include="I18n\*.json" />
</ItemGroup>
```

Generated shape:

```csharp
namespace Localization.Main;

public static class MainView
{
    public static readonly string Title = "Localization.Main.MainView.Title";
}
```

The field value must preserve the original resource key. Field names may be sanitized to produce valid C# identifiers.

## AXAML Usage

```xml
xmlns:c="https://codewf.com"
xmlns:mainLangs="clr-namespace:Localization.Main"

<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}}" />
<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}, CultureName=en-US}" />
```

Formatting arguments can be constants or Avalonia bindings:

```xml
<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.RunningCountInfo}, {Binding RunningCount}}" />
```

Dynamic keys can also come from bindings:

```xml
<SelectableTextBlock Text="{c:I18n {Binding SelectedResourceKey}}" />
```

## C# Usage

```csharp
var title = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
var titleEnUs = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "en-US");

I18nManager.Instance.Culture = new CultureInfo("ja-JP");
```

## Demo Projects

The current demos use a localization workspace scenario instead of a single-button or simple-text sample:

| Demo | Coverage |
| --- | --- |
| `Lang.Avalonia.Json.Demo` | JSON file scanning, T4 keys, runtime language switching |
| `Lang.Avalonia.Xml.Demo` | XML leaf-node parsing, T4 keys, fixed-culture preview |
| `Lang.Avalonia.Resx.Demo` | RESX Designer, ResourceManager, satellite resources |
| `Lang.Avalonia.Analysis.Demo` | JSON resources, `AdditionalFiles`, Source Generator |

## Maintenance Notes

1. Add XML documentation comments when adding public APIs.
2. Implement `ILangPlugin` for new resource formats; do not push format branching into the core library.
3. Keep demo resources and generated constants synchronized so XAML `x:Static` references remain valid.
4. Documentation examples should use the current `Localization.Main.MainView` resource structure and avoid historical business keys.
5. When updating SVG diagrams, render them in a browser and check arrow endpoints and text overlap.
