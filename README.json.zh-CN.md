# Lang.Avalonia.Json

简体中文 | [English](README.json.md)

Lang.Avalonia 的 JSON 资源提供器。它会扫描应用输出目录中的 JSON 语言文件，也可以从额外程序集加载嵌入的 JSON 资源。

## 安装

```shell
dotnet add package Lang.Avalonia
dotnet add package Lang.Avalonia.Json
```

## 资源文件

每个文化一个文件，并复制到输出目录：

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

## 初始化

```csharp
using Lang.Avalonia;
using Lang.Avalonia.Json;
using System.Globalization;

I18nManager.Instance.Register(new JsonLangPlugin(), new CultureInfo("en-US"), out var error);
```

如果发现无效 JSON 文件，`JsonLangPlugin.LoadDiagnostics` 会包含被跳过文件的诊断信息。

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
