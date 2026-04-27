# Lang.Avalonia 设计文档

本文档描述 Lang.Avalonia 的架构、资源加载、运行时解析流程和示例项目组织方式。文中的图示均以独立 SVG 文件存放在 `docs/assets`，方便在 README、NuGet 文档或站点中复用。

## 目标

Lang.Avalonia 的目标是在 Avalonia 应用中提供统一的多语言入口：XAML 使用 `{c:I18n}` 标记扩展，C# 使用 `I18nManager.Instance.GetResource`，资源格式由插件决定。核心库不关心资源来自 JSON、XML 还是 RESX，只依赖 `ILangPlugin`。

![总体架构](assets/architecture.svg)

## 包与职责

| 包 | 职责 |
| --- | --- |
| `Lang.Avalonia` | 标记扩展、绑定、转换器、`I18nManager`、插件接口 |
| `Lang.Avalonia.Json` | 加载 JSON 语言文件或嵌入 JSON 资源 |
| `Lang.Avalonia.Xml` | 加载 XML 语言文件或嵌入 XML 资源 |
| `Lang.Avalonia.Resx` | 从 `ResourceManager` 同步 RESX 资源 |
| `Lang.Avalonia.Analysis` | 编译期扫描语言文件并生成强类型 Key |

## 插件契约

插件必须实现 `ILangPlugin`，负责把不同来源的语言资源归一化为 `LocalizationLanguage` 缓存。核心库通过 `Load` 初始化默认语言，通过 `Culture` 切换当前语言，通过 `GetResource` 查询翻译文本。

![插件契约](assets/plugin-contract.svg)

关键约束：

1. `Load(defaultCulture)` 应设置默认语言并建立资源缓存。
2. `AddResource(assemblies)` 用于补充外部模块资源，JSON/XML 插件支持读取嵌入资源，RESX 插件从程序集类型中发现 `ResourceManager`。
3. `GetResource(key, cultureName)` 必须按指定文化、当前文化、默认文化的顺序解析，未命中时返回原始 Key。

## 使用流程

使用方先选择资源格式并安装对应插件包，然后创建语言资源，生成强类型 Key，在 `App.Initialize` 中注册插件，最后在 XAML 或后台代码中读取资源。

![使用流程](assets/usage-flow.svg)

典型初始化：

```csharp
I18nManager.Instance.Register(new JsonLangPlugin(), new CultureInfo("zh-CN"), out var error);
if (error != null)
{
    // 记录或展示初始化失败原因
}
```

典型 XAML：

```xml
<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}}" />
<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}, CultureName=en-US}" />
```

典型后台调用：

```csharp
var title = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
var titleEnUs = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "en-US");
```

## 资源解析流程

`I18nConverter` 会监听 `I18nManager.Culture`。当语言切换时，绑定重新求值，并通过当前插件获取资源。带参数文本使用 `string.Format(culture, format, args)` 格式化；绑定参数未就绪时保持现有值，避免 UI 初始绑定阶段出现异常。

![资源解析](assets/resource-resolution.svg)

Fallback 顺序：

1. 显式 `CultureName`，如果未设置则使用 `I18nManager.Culture`。
2. 初始化时传入的默认文化。
3. 原始 Key。

## 强类型 Key 生成

项目提供两条路径生成强类型 Key：示例项目中的 T4 模板，以及 `Lang.Avalonia.Analysis` Source Generator。设计上字段名可以为了 C# 标识符合法性而清洗，但字段值必须保留原始资源 Key，否则运行时查询会失败。

![强类型 Key 生成](assets/source-generation.svg)

Source Generator 输入来自 `AdditionalFiles`：

```xml
<AdditionalFiles Include="I18n\*.json" />
```

生成结果用于 XAML：

```xml
<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.ChangeLanguage}}" />
```

## 示例项目

| 示例 | 说明 |
| --- | --- |
| `Lang.Avalonia.Json.Demo` | JSON 文件输出到运行目录，插件扫描并加载 |
| `Lang.Avalonia.Xml.Demo` | XML 文件输出到运行目录，插件扫描并加载 |
| `Lang.Avalonia.Resx.Demo` | RESX 编译为程序集资源，插件通过 `ResourceManager` 读取 |
| `Lang.Avalonia.Analysis.Demo` | 使用 `Lang.Avalonia.Analysis` 在编译期生成 Key |

示例 ViewModel 统一使用非空语言列表，并在用户选择语言时判空后再切换 `I18nManager.Instance.Culture`。这避免了未加载资源或 ComboBox 清空选择时的空引用问题。

## 运行时注意事项

1. JSON/XML 插件默认扫描 `AppDomain.CurrentDomain.BaseDirectory`，示例项目通过 `CopyToOutputDirectory` 输出语言文件。
2. RESX 插件根据 `Mark` 过滤资源类型，默认值为 `i18n`，项目资源命名空间应包含该标记。
3. `I18nManager.Register` 会同步当前线程和默认线程的 Culture，后台任务中新建线程也能继承默认文化。
4. 动态 Key 绑定依赖 Avalonia `Binding`，启用裁剪时仍需要关注 Avalonia 对反射绑定的裁剪提示。
