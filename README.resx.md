# Lang.Avalonia.Resx

## 安装

```shell
Install-Package Lang.Avalonia.Resx
```

## 创建语言文件

参考示例项目创建XML语言文件

```shell
i18n/Resources.resx
i18n/Resources.zh-CN.resx
i18n/Resources.zh-Hant.resx
i18n/Resources.ja-JP.resx
```

## 创建T4文件

可直接复制示例T4文件使用，用于根据语言文件生成C#的常量语言Key，方便使用时引用

## 使用

初始化

```csharp
I18nManager.Instance.Register(new ResxLangPlugin(), new CultureInfo("zh-CN"), out _);
```

前台使用

```xml
xmlns:c="https://codewf.com"
xmlns:mainLangs="clr-namespace:Localization.Main"
xmlns:developModuleLanguage="clr-namespace:Localization.DevelopModule"

<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:SettingView.Title}}" />
<SelectableTextBlock Text="{c:I18n {x:Static developModuleLanguage:Title2SlugView.Title}, CultureName=zh-CN}" 
```

后台使用

```csharp
var titleCurrentCulture = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
var titleZhCN = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "zh-CN");
var titleEnUS = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "en-US");
```