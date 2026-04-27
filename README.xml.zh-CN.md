# Lang.Avalonia.Xml

简体中文 | [English](README.xml.md)

Lang.Avalonia 的 XML 资源提供器。它会扫描应用输出目录中的 XML 语言文件，也可以从额外程序集加载嵌入的 XML 资源。

## 安装

```shell
dotnet add package Lang.Avalonia
dotnet add package Lang.Avalonia.Xml
```

## 资源文件

每个文化一个文件，并复制到输出目录：

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

## 初始化

```csharp
using Lang.Avalonia;
using Lang.Avalonia.Xml;
using System.Globalization;

I18nManager.Instance.Register(new XmlLangPlugin(), new CultureInfo("en-US"), out var error);
```

如果发现无效 XML 文件，`XmlLangPlugin.LoadDiagnostics` 会包含被跳过文件的诊断信息。

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
