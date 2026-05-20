# Lang.Avalonia

English | [简体中文](README.zh-CN.md)

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
dotnet add package Lang.Avalonia.Json
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

Use generated constants in AXAML:

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

## JSON Provider

Install:

```shell
dotnet add package Lang.Avalonia.Json
```

Use one file per culture and copy JSON files to the output directory:

```text
I18n/en-US.json
I18n/zh-CN.json
I18n/zh-Hant.json
I18n/ja-JP.json
```

```xml
<ItemGroup>
  <None Update="I18n\*.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Each JSON file must include `language`, `description`, and `cultureName` metadata:

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

`JsonLangPlugin.LoadDiagnostics` contains skipped-file diagnostics if invalid JSON files are found.

## XML Provider

Install:

```shell
dotnet add package Lang.Avalonia.Xml
```

Use one file per culture and copy XML files to the output directory:

```text
I18n/en-US.xml
I18n/zh-CN.xml
I18n/zh-Hant.xml
I18n/ja-JP.xml
```

```xml
<ItemGroup>
  <None Update="I18n\*.xml" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Each XML file must include `language`, `description`, and `cultureName` metadata on the root node:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Localization language="English" description="English resources" cultureName="en-US">
  <Main>
    <MainView>
      <Title>Lang.Avalonia localization workspace</Title>
      <ChangeLanguage>Language</ChangeLanguage>
    </MainView>
  </Main>
</Localization>
```

Leaf-node paths become resource keys. The example above produces `Localization.Main.MainView.Title`.
`XmlLangPlugin.LoadDiagnostics` contains skipped-file diagnostics if invalid XML files are found.

## RESX Provider

Install:

```shell
dotnet add package Lang.Avalonia.Resx
```

Use standard .NET RESX naming:

```text
I18n/Resources.resx
I18n/Resources.zh-CN.resx
I18n/Resources.zh-Hant.resx
I18n/Resources.ja-JP.resx
```

Use full resource keys as RESX data names:

```xml
<data name="Localization.Main.MainView.Title" xml:space="preserve">
  <value>Lang.Avalonia localization workspace</value>
</data>
```

`ResxLangPlugin` syncs resources by culture and exposes them through the same `ILangPlugin` contract used by JSON and XML providers. For trimmed publishing, pass the generated `ResourceManager` explicitly so the app does not need a linker root file for Lang.Avalonia.Resx:

```csharp
using MyApp.I18n;

I18nManager.Instance.Register(
    new ResxLangPlugin(Resources.ResourceManager),
    new CultureInfo("en-US"),
    out var error);
```

You can also pass the generated resource designer type:

```csharp
I18nManager.Instance.Register(
    new ResxLangPlugin(typeof(Resources)),
    new CultureInfo("en-US"),
    out var error);
```

The default `ResxLangPlugin.Mark` value is `i18n`; keep generated resource designer types under a namespace or folder that contains `I18n`, or set `Mark` explicitly:

```csharp
I18nManager.Instance.Register(
    new ResxLangPlugin { Mark = "Resources" },
    new CultureInfo("en-US"),
    out var error);
```

Convention-based discovery is kept for compatibility, but explicit registration is the recommended path for trimmed apps.

## Type-Safe Keys

Two generation paths are supported:

- Demo T4 templates generate `I18n/Language.cs` from JSON, XML, or RESX resources.
- `Lang.Avalonia.Analysis` generates `Language.g.cs` at compile time from `AdditionalFiles`.

For application projects, keep `Lang.Avalonia.Analysis` private because it is a build-time analyzer:

```xml
<PackageReference Include="Lang.Avalonia.Analysis" Version="*" PrivateAssets="all" />
```

Register JSON, XML, or RESX language files as `AdditionalFiles`:

```xml
<ItemGroup>
  <AdditionalFiles Include="I18n\*.json" />
  <AdditionalFiles Include="I18n\*.xml" />
  <AdditionalFiles Include="I18n\*.resx" />
</ItemGroup>
```

Generated field values preserve the original resource keys. Only C# identifier names are sanitized.

For a resource key such as:

```text
Localization.Main.MainView.Title
```

the generator emits constants shaped like:

```csharp
namespace Localization.Main;

public static class MainView
{
    public static readonly string Title = "Localization.Main.MainView.Title";
}
```

## Demos And Design Notes

The repository contains four demos:

| Demo | Purpose |
| --- | --- |
| `Lang.Avalonia.Json.Demo` | JSON files copied to output and loaded by `JsonLangPlugin` |
| `Lang.Avalonia.Xml.Demo` | XML files copied to output and loaded by `XmlLangPlugin` |
| `Lang.Avalonia.Resx.Demo` | RESX resources loaded through `ResourceManager` |
| `Lang.Avalonia.Analysis.Demo` | JSON resources plus source-generated keys |

Design documentation and SVG diagrams are available in [docs/design.md](https://github.com/dotnet9/Lang.Avalonia/blob/main/docs/design.md).

## Fallback Rules

Resource lookup follows this order:

1. Explicit `CultureName` when provided; otherwise `I18nManager.Instance.Culture`.
2. The default culture passed to `Register`.
3. The original resource key.

## Notes

- JSON and XML providers scan `AppDomain.CurrentDomain.BaseDirectory` by default.
- JSON and XML providers can also read embedded resources through `AddResource`.
- RESX provider does not require a Root.xml file for trimmed publishing when registered with an explicit `ResourceManager` or resource designer type.
- Dynamic Avalonia bindings are supported for keys and format arguments.

## Third-Party Open Source Audit (2026-05-20)

Checked with `dotnet restore Lang.Avalonia.slnx`, `dotnet list package --include-transitive`, NuGet `.nuspec` metadata, NuGet.org, and upstream source repositories. MIT / Apache-2.0 / BSD are preferred; other source-open licenses are explicitly marked when source and transitive dependencies are traceable.

Remediation:

- Removed `AvaloniaUI.DiagnosticsSupport` from the four demo projects.
- Updated `Avalonia` / `Avalonia.Desktop` from `12.0.2` to `12.0.3`.
- Pinned `System.Drawing.Common` to `10.0.8`.
- Updated `System.Text.Json` from `10.0.2` to `10.0.8`.
- Kept `Prism.Avalonia`, `Prism.DryIoc.Avalonia`, and the matching `Irihi.Ursa.PrismExtension` integration on the existing open-source 8.x-compatible line instead of moving to the Prism 9.x commercial line.

| Package | Usage | License | Source | Status |
| --- | --- | --- | --- | --- |
| `Avalonia` / `Avalonia.Desktop` | Demo UI and core Avalonia integration | MIT | https://github.com/AvaloniaUI/Avalonia | Approved, updated to `12.0.3` |
| `Semi.Avalonia` | Demo theme | MIT | https://github.com/irihitech/Semi.Avalonia | Approved, only the open core package is used |
| `Irihi.Ursa` / `Irihi.Ursa.PrismExtension` / `Irihi.Ursa.Themes.Semi` | Demo controls and Prism integration | MIT | https://github.com/irihitech/Ursa.Avalonia | Approved, Prism extension kept on the existing 8.x-compatible line |
| `Prism.Avalonia` / `Prism.DryIoc.Avalonia` `8.1.97.11073` | Demo DI / Prism shell | MIT | https://github.com/AvaloniaCommunity/Prism.Avalonia | Approved, pinned to the 8.x open-source line |
| `ReactiveUI.Avalonia` | Demo MVVM | MIT | https://github.com/reactiveui/reactiveui | Approved |
| `Microsoft.CodeAnalysis.*` | `Lang.Avalonia.Analysis` source generation | MIT | https://github.com/dotnet/roslyn | Approved |
| `System.Drawing.Common` / `System.Text.Json` | RESX and JSON support | MIT | https://github.com/dotnet/dotnet | Approved, pinned to `10.0.8` |
| `VC-LTL` | Windows compatibility | EPL-2.0 | https://github.com/Chuyu-Team/VC-LTL5 | Source-open; approved under the source-traceable non-preferred license rule |
| `YY-Thunks` | Windows compatibility | MIT | https://github.com/Chuyu-Team/YY-Thunks | Source-open; approved |

Transitive dependency check: the Avalonia, Ursa, Semi, Prism, ReactiveUI, Roslyn, and .NET runtime/library chains are source-open. Active project files no longer contain `AvaloniaUI.DiagnosticsSupport` or other black-box components.
