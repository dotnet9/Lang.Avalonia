# Lang.Avalonia

简体中文 | [English](README.md)

Lang.Avalonia 是面向 Avalonia UI 的插件化多语言库。核心包提供 XAML 标记扩展、绑定刷新流程、转换器、`I18nManager` 和 `ILangPlugin` 契约；资源加载由 JSON、XML、RESX 插件实现，也可以使用 Source Generator 生成强类型资源 Key。

| 包 | NuGet | 下载量 |
| --- | --- | --- |
| Lang.Avalonia | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia)](https://www.nuget.org/packages/Lang.Avalonia/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia)](https://www.nuget.org/packages/Lang.Avalonia/) |
| Lang.Avalonia.Json | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Json.svg)](https://www.nuget.org/packages/Lang.Avalonia.Json/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Json.svg)](https://www.nuget.org/packages/Lang.Avalonia.Json/) |
| Lang.Avalonia.Xml | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Xml.svg)](https://www.nuget.org/packages/Lang.Avalonia.Xml/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Xml.svg)](https://www.nuget.org/packages/Lang.Avalonia.Xml/) |
| Lang.Avalonia.Resx | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Resx.svg)](https://www.nuget.org/packages/Lang.Avalonia.Resx/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Resx.svg)](https://www.nuget.org/packages/Lang.Avalonia.Resx/) |
| Lang.Avalonia.Analysis | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Analysis.svg)](https://www.nuget.org/packages/Lang.Avalonia.Analysis/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Analysis.svg)](https://www.nuget.org/packages/Lang.Avalonia.Analysis/) |

## 功能

- 统一的 XAML 与 C# 多语言入口。
- 通过 `I18nManager.Instance.Culture` 运行时切换语言。
- JSON、XML、RESX 资源提供器统一实现 `ILangPlugin`。
- 支持默认文化回退和原始 Key 回退。
- 支持 T4 模板或 `Lang.Avalonia.Analysis` 生成强类型资源 Key。
- 支持固定文化预览、格式化字符串和动态绑定参数。

## 选择包

| 资源格式 | 包 | 适用场景 |
| --- | --- | --- |
| JSON | `Lang.Avalonia` + `Lang.Avalonia.Json` | 可编辑文件、跨平台工具链、Source Generator 示例 |
| XML | `Lang.Avalonia` + `Lang.Avalonia.Xml` | 层级清晰的语言文件 |
| RESX | `Lang.Avalonia` + `Lang.Avalonia.Resx` | .NET ResourceManager 和卫星程序集 |
| 强类型 Key | `Lang.Avalonia.Analysis` | 从 `AdditionalFiles` 编译期生成常量 |

## 快速开始

安装核心包和一个资源提供器：

```shell
dotnet add package Lang.Avalonia
dotnet add package Lang.Avalonia.Json
```

在 `I18n` 目录创建语言文件，并将 JSON/XML 文件复制到输出目录：

```xml
<ItemGroup>
  <None Update="I18n\*.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

JSON 资源示例：

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

应用启动时注册插件：

```csharp
using Lang.Avalonia;
using Lang.Avalonia.Json;
using System.Globalization;

I18nManager.Instance.Register(new JsonLangPlugin(), new CultureInfo("en-US"), out var error);
if (!string.IsNullOrWhiteSpace(error))
{
    // 记录或展示初始化错误。
}
```

在 AXAML 中使用生成的常量：

```xml
xmlns:c="https://codewf.com"
xmlns:mainLangs="clr-namespace:Localization.Main"

<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}}" />
<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}, CultureName=ja-JP}" />
```

在 C# 中使用相同 Key：

```csharp
var title = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
var englishTitle = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "en-US");
```

运行时切换语言：

```csharp
I18nManager.Instance.Culture = new CultureInfo("zh-CN");
```

## 强类型 Key

支持两种生成路径：

- 示例项目中的 T4 模板会从 JSON、XML 或 RESX 资源生成 `I18n/Language.cs`。
- `Lang.Avalonia.Analysis` 会从 `AdditionalFiles` 在编译期生成 `Language.g.cs`。

Source Generator 示例：

```xml
<ItemGroup>
  <PackageReference Include="Lang.Avalonia.Analysis" Version="*" PrivateAssets="all" />
  <AdditionalFiles Include="I18n\*.json" />
</ItemGroup>
```

生成字段的值会保留原始资源 Key，只有 C# 标识符名称会被清洗。

## 示例和设计说明

仓库包含四个示例：

| 示例 | 用途 |
| --- | --- |
| `Lang.Avalonia.Json.Demo` | JSON 文件复制到输出目录并由 `JsonLangPlugin` 加载 |
| `Lang.Avalonia.Xml.Demo` | XML 文件复制到输出目录并由 `XmlLangPlugin` 加载 |
| `Lang.Avalonia.Resx.Demo` | 通过 `ResourceManager` 加载 RESX 资源 |
| `Lang.Avalonia.Analysis.Demo` | JSON 资源加 Source Generator 生成 Key |

设计文档和 SVG 图示见 [docs/design.md](https://github.com/dotnet9/Lang.Avalonia/blob/main/docs/design.md)。

## 回退规则

资源查找顺序如下：

1. 显式 `CultureName`；未提供时使用 `I18nManager.Instance.Culture`。
2. `Register` 时传入的默认文化。
3. 原始资源 Key。

## 注意事项

- JSON 和 XML 提供器默认扫描 `AppDomain.CurrentDomain.BaseDirectory`。
- JSON 和 XML 提供器也可以通过 `AddResource` 读取嵌入资源。
- RESX 提供器通过 `ResourceManager` 发现生成的资源 Designer 类型；默认类型过滤标记为 `i18n`。
- 资源 Key 和格式化参数均支持动态 Avalonia Binding。
