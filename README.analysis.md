# Lang.Avalonia.Analysis

[简体中文](README.analysis.zh-CN.md) | English

Source generator package for Lang.Avalonia. It scans language resources declared as `AdditionalFiles` and generates type-safe C# constants for use with `x:Static` and `I18nManager.GetResource`.

## Install

```shell
dotnet add package Lang.Avalonia.Analysis
```

For application projects, keep the package private because it is a build-time analyzer:

```xml
<PackageReference Include="Lang.Avalonia.Analysis" Version="*" PrivateAssets="all" />
```

## Configure Inputs

Register JSON, XML, or RESX language files as `AdditionalFiles`:

```xml
<ItemGroup>
  <AdditionalFiles Include="I18n\*.json" />
  <AdditionalFiles Include="I18n\*.xml" />
  <AdditionalFiles Include="I18n\*.resx" />
</ItemGroup>
```

JSON and XML files still need to be copied to output when the runtime plugin loads them from disk:

```xml
<ItemGroup>
  <None Update="I18n\*.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## Generated Shape

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

The generated field value always preserves the original resource key. Only the C# field name is sanitized when the key segment is not a valid identifier.

## Use

```xml
xmlns:c="https://codewf.com"
xmlns:mainLangs="clr-namespace:Localization.Main"

<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}}" />
```

```csharp
var title = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
```
