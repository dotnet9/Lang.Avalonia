# Lang.Avalonia.Resx

简体中文 | [English](README.resx.md)

Lang.Avalonia 的 RESX 资源提供器。它按约定发现生成的 `ResourceManager` 属性，按文化同步资源，并通过与 JSON、XML 提供器相同的 `ILangPlugin` 契约对外提供资源。

## 安装

```shell
dotnet add package Lang.Avalonia
dotnet add package Lang.Avalonia.Resx
```

## 资源文件

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

`ResxLangPlugin.Mark` 默认值为 `i18n`。请让生成的资源 Designer 类型位于包含 `I18n` 的命名空间或文件夹下，或者显式设置 `Mark`。

## 初始化

```csharp
using Lang.Avalonia;
using Lang.Avalonia.Resx;
using System.Globalization;

I18nManager.Instance.Register(new ResxLangPlugin(), new CultureInfo("en-US"), out var error);
```

自定义标记示例：

```csharp
I18nManager.Instance.Register(
    new ResxLangPlugin { Mark = "Resources" },
    new CultureInfo("en-US"),
    out var error);
```

## 在 XAML 中使用

```xml
xmlns:c="https://codewf.com"
xmlns:mainLangs="clr-namespace:Localization.Main"

<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}}" />
<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:MainView.Title}, CultureName=zh-CN}" />
```

## 在 C# 中使用

```csharp
var title = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
var titleJaJp = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "ja-JP");
```
