# Lang.Avalonia

Lang.Avalonia 是面向 Avalonia UI 的插件化多语言库。核心包提供 XAML 标记扩展、绑定刷新流程、转换器、`I18nManager` 和 `ILangPlugin` 契约；资源加载由 JSON、XML、RESX 插件实现，也可以使用 Source Generator 生成强类型资源 Key。

| 包 | NuGet | 下载量 |
| --- | --- | --- |
| Lang.Avalonia | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia)](https://www.nuget.org/packages/Lang.Avalonia/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia)](https://www.nuget.org/packages/Lang.Avalonia/) |
| Lang.Avalonia.Json | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Json.svg)](https://www.nuget.org/packages/Lang.Avalonia.Json/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Json.svg)](https://www.nuget.org/packages/Lang.Avalonia.Json/) |
| Lang.Avalonia.Xml | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Xml.svg)](https://www.nuget.org/packages/Lang.Avalonia.Xml/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Xml.svg)](https://www.nuget.org/packages/Lang.Avalonia.Xml/) |
| Lang.Avalonia.Resx | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Resx.svg)](https://www.nuget.org/packages/Lang.Avalonia.Resx/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Resx.svg)](https://www.nuget.org/packages/Lang.Avalonia.Resx/) |
| Lang.Avalonia.Analysis | [![NuGet](https://img.shields.io/nuget/v/Lang.Avalonia.Analysis.svg)](https://www.nuget.org/packages/Lang.Avalonia.Analysis/) | [![NuGet](https://img.shields.io/nuget/dt/Lang.Avalonia.Analysis.svg)](https://www.nuget.org/packages/Lang.Avalonia.Analysis/) |

## 仓库规范

- 当前版本：`12.0.3.5`，版本号统一维护在根目录 `Directory.Build.props` 的 `<Version>` 节点。
- NuGet 包项目统一支持 `net8.0;net10.0`；Demo、App、测试与内部应用项目统一使用 `net11.0` / `net11.0-windows`。
- 根目录 `logo.svg`、`logo.png`、`logo.ico` 是唯一图标源，子工程只通过 MSBuild `Link` 引用，不维护图标副本。
- 运行时帮助、Markdown 示例、内置备忘录、设计说明等业务文档按功能保留；仓库级入口文档使用根目录 `README.md` 和 `UpdateLog.md`。

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
dotnet add package Lang.Avalonia.Json
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

## JSON 提供器

安装：

```shell
dotnet add package Lang.Avalonia.Json
```

每个文化一个文件，并将 JSON 文件复制到输出目录：

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

每个 JSON 文件都必须包含 `language`、`description`、`cultureName` 元数据：

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

如果发现无效 JSON 文件，`JsonLangPlugin.LoadDiagnostics` 会包含被跳过文件的诊断信息。

## XML 提供器

安装：

```shell
dotnet add package Lang.Avalonia.Xml
```

每个文化一个文件，并将 XML 文件复制到输出目录：

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

每个 XML 文件都必须在根节点上包含 `language`、`description`、`cultureName` 元数据：

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

叶子节点路径会成为资源 Key。上面的示例会生成 `Localization.Main.MainView.Title`。
如果发现无效 XML 文件，`XmlLangPlugin.LoadDiagnostics` 会包含被跳过文件的诊断信息。

## RESX 提供器

安装：

```shell
dotnet add package Lang.Avalonia.Resx
```

使用标准 .NET RESX 命名：

```text
I18n/Resources.resx
I18n/Resources.zh-CN.resx
I18n/Resources.zh-Hant.resx
I18n/Resources.ja-JP.resx
```

建议使用完整资源 Key 作为 RESX 数据名称：

```xml
<data name="Localization.Main.MainView.Title" xml:space="preserve">
  <value>Lang.Avalonia localization workspace</value>
</data>
```

`ResxLangPlugin` 按文化同步资源，并通过与 JSON、XML 提供器相同的 `ILangPlugin` 契约对外提供资源。裁剪发布时建议显式传入生成的 `ResourceManager`，这样应用不需要为了 Lang.Avalonia.Resx 额外配置链接器 Root 文件：

```csharp
using MyApp.I18n;

I18nManager.Instance.Register(
    new ResxLangPlugin(Resources.ResourceManager),
    new CultureInfo("zh-CN"),
    out var error);
```

也可以传入生成的资源 Designer 类型：

```csharp
I18nManager.Instance.Register(
    new ResxLangPlugin(typeof(Resources)),
    new CultureInfo("zh-CN"),
    out var error);
```

`ResxLangPlugin.Mark` 默认值为 `i18n`。请让生成的资源 Designer 类型位于包含 `I18n` 的命名空间或文件夹下，或者显式设置 `Mark`：

```csharp
I18nManager.Instance.Register(
    new ResxLangPlugin { Mark = "Resources" },
    new CultureInfo("en-US"),
    out var error);
```

按约定扫描保留用于兼容旧用法，但裁剪应用推荐使用显式注册。

## 强类型 Key

支持两种生成路径：

- 示例项目中的 T4 模板会从 JSON、XML 或 RESX 资源生成 `I18n/Language.cs`。
- `Lang.Avalonia.Analysis` 会从 `AdditionalFiles` 在编译期生成 `Language.g.cs`。

在应用项目中建议将 `Lang.Avalonia.Analysis` 作为私有构建期分析器：

```xml
<PackageReference Include="Lang.Avalonia.Analysis" Version="*" PrivateAssets="all" />
```

将 JSON、XML 或 RESX 语言文件注册为 `AdditionalFiles`：

```xml
<ItemGroup>
  <AdditionalFiles Include="I18n\*.json" />
  <AdditionalFiles Include="I18n\*.xml" />
  <AdditionalFiles Include="I18n\*.resx" />
</ItemGroup>
```

生成字段的值会保留原始资源 Key，只有 C# 标识符名称会被清洗。

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
- RESX 提供器在显式注册 `ResourceManager` 或资源 Designer 类型时，裁剪发布不需要为 Lang.Avalonia.Resx 配置 Root.xml。
- 资源 Key 和格式化参数均支持动态 Avalonia Binding。

## 第三方开源组件审计（2026-05-20）

检查方式：`dotnet restore Lang.Avalonia.slnx`、`dotnet list package --include-transitive`、NuGet `.nuspec`、NuGet.org 与源码仓库信息。优先接受 MIT / Apache-2.0 / BSD；其它开源协议在源码与传递依赖均可追溯时单独标注。

整改：

- 四个 Demo 已移除 `AvaloniaUI.DiagnosticsSupport`。
- `Avalonia` / `Avalonia.Desktop` 从 `12.0.2` 升级到 `12.0.3`。
- `System.Drawing.Common` 固定到 `10.0.8`。
- `System.Text.Json` 从 `10.0.2` 升级到 `10.0.8`。
- `Prism.Avalonia`、`Prism.DryIoc.Avalonia` 以及配套的 `Irihi.Ursa.PrismExtension` 继续保留在当前 8.x 兼容开源线，不升级到 Prism 9.x 商业化版本线。

| 包 | 使用范围 | 协议 | 源码/项目地址 | 结论 |
| --- | --- | --- | --- | --- |
| `Avalonia` / `Avalonia.Desktop` | Demo UI 与核心 Avalonia 集成 | MIT | https://github.com/AvaloniaUI/Avalonia | 通过，已升级到 `12.0.3` |
| `Semi.Avalonia` | Demo 主题 | MIT | https://github.com/irihitech/Semi.Avalonia | 通过，仅使用开源主体包 |
| `Irihi.Ursa` / `Irihi.Ursa.PrismExtension` / `Irihi.Ursa.Themes.Semi` | Demo 控件与 Prism 扩展 | MIT | https://github.com/irihitech/Ursa.Avalonia | 通过，Prism 扩展保留在当前 8.x 兼容线 |
| `Prism.Avalonia` / `Prism.DryIoc.Avalonia` `8.1.97.11073` | Demo DI / Prism shell | MIT | https://github.com/AvaloniaCommunity/Prism.Avalonia | 通过，保留 8.x 开源线 |
| `ReactiveUI.Avalonia` | Demo MVVM | MIT | https://github.com/reactiveui/reactiveui | 通过 |
| `Microsoft.CodeAnalysis.*` | `Lang.Avalonia.Analysis` 源码生成 | MIT | https://github.com/dotnet/roslyn | 通过 |
| `System.Drawing.Common` / `System.Text.Json` | RESX 与 JSON 支持 | MIT | https://github.com/dotnet/dotnet | 通过，固定到 `10.0.8` |
| `VC-LTL` | Windows 兼容 | EPL-2.0 | https://github.com/Chuyu-Team/VC-LTL5 | 源码开放，按“非优先但可追溯”通过 |
| `YY-Thunks` | Windows 兼容 | MIT | https://github.com/Chuyu-Team/YY-Thunks | 源码开放，通过 |

传递依赖检查结论：Avalonia / Ursa / Semi / Prism / ReactiveUI / Roslyn / .NET 运行时与类库链路均有公开源码；有效项目文件中不再包含 `AvaloniaUI.DiagnosticsSupport` 或其它黑盒组件。
