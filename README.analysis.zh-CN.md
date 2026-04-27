# Lang.Avalonia.Analysis

简体中文 | [English](README.analysis.md)

Lang.Avalonia 的 Source Generator 包。它扫描声明为 `AdditionalFiles` 的语言资源，并生成可用于 `x:Static` 和 `I18nManager.GetResource` 的强类型 C# 常量。

## 安装

```shell
dotnet add package Lang.Avalonia.Analysis
```

在应用项目中建议将该包作为私有构建期分析器：

```xml
<PackageReference Include="Lang.Avalonia.Analysis" Version="*" PrivateAssets="all" />
```

## 配置输入

将 JSON、XML 或 RESX 语言文件注册为 `AdditionalFiles`：

```xml
<ItemGroup>
  <AdditionalFiles Include="I18n\*.json" />
  <AdditionalFiles Include="I18n\*.xml" />
  <AdditionalFiles Include="I18n\*.resx" />
</ItemGroup>
```

如果运行时插件需要从磁盘加载 JSON/XML 文件，这些文件仍然需要复制到输出目录：

```xml
<ItemGroup>
  <None Update="I18n\*.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## 生成形态

对于以下资源 Key：

```text
Localization.Main.MainView.Title
```

生成器会输出类似常量：

```csharp
namespace Localization.Main;

public static class MainView
{
    public static readonly string Title = "Localization.Main.MainView.Title";
}
```

生成字段的值始终保留原始资源 Key。只有当 Key 片段不是合法 C# 标识符时，字段名才会被清洗。

## 使用

```xml
xmlns:c="https://codewf.com"
xmlns:mainLangs="clr-namespace:Localization.Main"

<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}}" />
```

```csharp
var title = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
```
