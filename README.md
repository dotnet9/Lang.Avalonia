# Lang.Avalonia

[简体中文](README.zh-CN.md) | English

Lang.Avalonia is a plugin-based localization library for Avalonia UI. The core package provides the XAML markup extension, binding pipeline, converters, `I18nManager`, and the `ILangPlugin` contract. Resource loading is provided by JSON, XML, and RESX plugins, with an optional source generator for type-safe resource keys.

| Package | NuGet | Downloads |
| --- | --- | --- |
| Lang.Avalonia | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia)](https://www.nuget.org/packages/Lang.Avalonia/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia)](https://www.nuget.org/packages/Lang.Avalonia/) |
| Lang.Avalonia.Json | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Json.svg)](https://www.nuget.org/packages/Lang.Avalonia.Json/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Json.svg)](https://www.nuget.org/packages/Lang.Avalonia.Json/) |
| Lang.Avalonia.Xml | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Xml.svg)](https://www.nuget.org/packages/Lang.Avalonia.Xml/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Xml.svg)](https://www.nuget.org/packages/Lang.Avalonia.Xml/) |
| Lang.Avalonia.Resx | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Resx.svg)](https://www.nuget.org/packages/Lang.Avalonia.Resx/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Resx.svg)](https://www.nuget.org/packages/Lang.Avalonia.Resx/) |
| Lang.Avalonia.Analysis | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Analysis.svg)](https://www.nuget.org/packages/Lang.Avalonia.Analysis/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Analysis.svg)](https://www.nuget.org/packages/Lang.Avalonia.Analysis/) |

## Features

- Unified XAML and C# localization entry points.
- Runtime language switching through `I18nManager.Instance.Culture`.
- JSON, XML, and RESX resource providers behind the same `ILangPlugin` contract.
- Default-culture fallback and original-key fallback.
- Type-safe resource key generation with T4 templates or `Lang.Avalonia.Analysis`.
- Support for fixed-culture preview bindings, formatted strings, and dynamic binding arguments.

## Package Selection

| Resource format | Packages | Typical use |
| --- | --- | --- |
| JSON | `Lang.Avalonia` + `Lang.Avalonia.Json` | Editable files, cross-platform tooling, source-generator demos |
| XML | `Lang.Avalonia` + `Lang.Avalonia.Xml` | Structured language files with nested nodes |
| RESX | `Lang.Avalonia` + `Lang.Avalonia.Resx` | .NET ResourceManager and satellite assemblies |
| Type-safe keys | `Lang.Avalonia.Analysis` | Compile-time constants from `AdditionalFiles` |

## Quick Start

Install the core package and one provider:

```shell
dotnet add package Lang.Avalonia
dotnet add package Lang.Avalonia.Json
```

Create language files under `I18n` and copy JSON/XML files to the output directory:

```xml
<ItemGroup>
  <None Update="I18n\*.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Example JSON resource:

```json
{
  "language": "English",
  "description": "English resources",
  "cultureName": "en-US",
  "Localization": {
    "Main": {
      "MainView": {
        "Title": "Lang.Avalonia localization workspace",
        "ChangeLanguage": "Language"
      }
    }
  }
}
```

Register the plugin during app startup:

```csharp
using Lang.Avalonia;
using Lang.Avalonia.Json;
using System.Globalization;

I18nManager.Instance.Register(new JsonLangPlugin(), new CultureInfo("en-US"), out var error);
if (!string.IsNullOrWhiteSpace(error))
{
    // Log or show the initialization error.
}
```

Use the generated constants in AXAML:

```xml
xmlns:c="https://codewf.com"
xmlns:mainLangs="clr-namespace:Localization.Main"

<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}}" />
<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}, CultureName=ja-JP}" />
```

Use the same keys from C#:

```csharp
var title = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
var englishTitle = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "en-US");
```

Switch language at runtime:

```csharp
I18nManager.Instance.Culture = new CultureInfo("zh-CN");
```

## Type-Safe Keys

Two generation paths are supported:

- Demo T4 templates generate `I18n/Language.cs` from JSON, XML, or RESX resources.
- `Lang.Avalonia.Analysis` generates `Language.g.cs` at compile time from `AdditionalFiles`.

Source generator example:

```xml
<ItemGroup>
  <PackageReference Include="Lang.Avalonia.Analysis" Version="*" PrivateAssets="all" />
  <AdditionalFiles Include="I18n\*.json" />
</ItemGroup>
```

Generated field values preserve the original resource keys. Only C# identifier names are sanitized.

## Demos And Design Notes

The repository contains four demos:

| Demo | Purpose |
| --- | --- |
| `Lang.Avalonia.Json.Demo` | JSON files copied to output and loaded by `JsonLangPlugin` |
| `Lang.Avalonia.Xml.Demo` | XML files copied to output and loaded by `XmlLangPlugin` |
| `Lang.Avalonia.Resx.Demo` | RESX resources loaded through `ResourceManager` |
| `Lang.Avalonia.Analysis.Demo` | JSON resources plus source-generated keys |

Design documentation and SVG diagrams are available in [docs/design.en-US.md](https://github.com/dotnet9/Lang.Avalonia/blob/main/docs/design.en-US.md).

## Fallback Rules

Resource lookup follows this order:

1. Explicit `CultureName` when provided; otherwise `I18nManager.Instance.Culture`.
2. The default culture passed to `Register`.
3. The original resource key.

## Notes

- JSON and XML providers scan `AppDomain.CurrentDomain.BaseDirectory` by default.
- JSON and XML providers can also read embedded resources through `AddResource`.
- RESX provider discovers generated resource designer types by `ResourceManager`; the default type filter mark is `i18n`.
- Dynamic Avalonia bindings are supported for keys and format arguments.
