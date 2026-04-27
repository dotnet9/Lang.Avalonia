# Lang.Avalonia 技术说明

简体中文 | [English](Lang.Avalonia.en-US.md)

本文是面向维护者和集成方的技术说明。快速上手请先看 [README.md](README.md)，架构图和流程图请看 [docs/design.md](docs/design.md)。

## 设计目标

Lang.Avalonia 将 Avalonia 应用的本地化拆成两层：

1. `Lang.Avalonia` 核心库只负责 XAML 标记扩展、绑定刷新、格式化和文化切换。
2. JSON、XML、RESX 插件负责把不同来源的资源归一化为 `LocalizationLanguage` 字典。

这种设计避免 UI、ViewModel 和业务代码关心资源文件格式。使用方只需要注册一个 `ILangPlugin`，之后都通过 `{c:I18n}` 或 `I18nManager.Instance.GetResource` 读取文本。

## 核心类型

| 类型 | 所属包 | 作用 |
| --- | --- | --- |
| `I18nManager` | `Lang.Avalonia` | 全局运行时入口，负责注册插件、切换 Culture、触发绑定刷新 |
| `I18nExtension` | `Lang.Avalonia` | AXAML 标记扩展入口，语法为 `{c:I18n ...}` |
| `I18nBinding` | `Lang.Avalonia` | 监听 Culture、资源 Key 和格式化参数的多值绑定 |
| `I18nConverter` | `Lang.Avalonia` | 执行资源查找、`string.Format` 和最终值转换 |
| `ILangPlugin` | `Lang.Avalonia` | 插件契约，屏蔽 JSON、XML、RESX 的加载差异 |
| `JsonLangPlugin` | `Lang.Avalonia.Json` | 扫描 JSON 文件或嵌入 JSON 资源 |
| `XmlLangPlugin` | `Lang.Avalonia.Xml` | 扫描 XML 文件或嵌入 XML 资源 |
| `ResxLangPlugin` | `Lang.Avalonia.Resx` | 发现 `ResourceManager` 并同步 RESX 资源 |
| `LanguageSourceGenerator` | `Lang.Avalonia.Analysis` | 从 `AdditionalFiles` 生成强类型资源 Key |

## 运行时流程

1. 应用启动时调用 `I18nManager.Instance.Register(plugin, defaultCulture, out error)`。
2. 插件执行 `Load(defaultCulture)`，建立资源缓存。
3. `I18nManager` 同步当前线程和默认线程的 `CurrentCulture` / `CurrentUICulture`。
4. AXAML 中的 `{c:I18n}` 绑定监听 `I18nManager.Culture`。
5. 当 `Culture` 改变时，所有绑定重新通过插件获取资源。
6. 插件按显式文化、当前文化、默认文化、原始 Key 的顺序回退。

## JSON 资源

JSON 文件适合希望语言文件可编辑、可由外部工具处理的应用。每个文件必须提供三个元数据字段：

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

插件会将叶子节点展开为点分隔 Key，例如：

```text
Localization.Main.MainView.Title
```

项目文件需要确保 JSON 被复制到输出目录：

```xml
<ItemGroup>
  <None Update="I18n\*.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## XML 资源

XML 文件适合层级结构更明确的资源。根节点保存语言元数据，叶子节点作为资源值：

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

同样需要复制到输出目录：

```xml
<ItemGroup>
  <None Update="I18n\*.xml" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## RESX 资源

RESX 文件使用标准 .NET `ResourceManager` 机制。资源名建议直接使用完整 Key：

```xml
<data name="Localization.Main.MainView.Title" xml:space="preserve">
  <value>Lang.Avalonia localization workspace</value>
</data>
```

`ResxLangPlugin` 会扫描已加载程序集中的资源 Designer 类型，并读取其 `ResourceManager` 属性。默认 `Mark` 为 `i18n`，用于过滤资源类型名称；如果项目资源命名不包含 `I18n`，可显式设置：

```csharp
new ResxLangPlugin { Mark = "Resources" }
```

## 强类型 Key

示例项目提供 T4 模板，`Lang.Avalonia.Analysis` 提供 Source Generator。二者目标一致：生成可用于 `x:Static` 的常量，避免手写字符串。

Source Generator 配置示例：

```xml
<ItemGroup>
  <PackageReference Include="Lang.Avalonia.Analysis" Version="*" PrivateAssets="all" />
  <AdditionalFiles Include="I18n\*.json" />
</ItemGroup>
```

生成形态：

```csharp
namespace Localization.Main;

public static class MainView
{
    public static readonly string Title = "Localization.Main.MainView.Title";
}
```

注意：字段值必须保持原始资源 Key；字段名可以为了 C# 标识符合法性做清洗。

## AXAML 使用

```xml
xmlns:c="https://codewf.com"
xmlns:mainLangs="clr-namespace:Localization.Main"

<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}}" />
<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}, CultureName=en-US}" />
```

格式化参数支持常量和 Avalonia Binding：

```xml
<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.RunningCountInfo}, {Binding RunningCount}}" />
```

动态 Key 也可以来自绑定：

```xml
<SelectableTextBlock Text="{c:I18n {Binding SelectedResourceKey}}" />
```

## C# 使用

```csharp
var title = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
var titleEnUs = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "en-US");

I18nManager.Instance.Culture = new CultureInfo("ja-JP");
```

## 示例项目

当前示例统一展示“本地化工作台”场景，而不是单一按钮或简单文本：

| 示例 | 覆盖点 |
| --- | --- |
| `Lang.Avalonia.Json.Demo` | JSON 文件扫描、T4 Key、运行时语言切换 |
| `Lang.Avalonia.Xml.Demo` | XML 叶子节点解析、T4 Key、固定文化预览 |
| `Lang.Avalonia.Resx.Demo` | RESX Designer、ResourceManager、卫星资源 |
| `Lang.Avalonia.Analysis.Demo` | JSON 资源、`AdditionalFiles`、Source Generator |

## 维护建议

1. 新增公开 API 时同步补充 XML 文档注释。
2. 新增资源格式时优先实现 `ILangPlugin`，不要把格式判断放进核心库。
3. 示例资源和生成常量必须保持同步，避免 XAML 的 `x:Static` 指向不存在的 Key。
4. 文档示例应使用 `Localization.Main.MainView` 等当前资源结构，避免引用历史业务 Key。
5. 更新 SVG 图时使用浏览器渲染复查箭头端点和文字重叠。
