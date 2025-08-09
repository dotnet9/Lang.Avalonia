# Lang.Avalonia.Json

## ��װ

```shell
Install-Package Lang.Avalonia.Json
```

## ���������ļ�

�ο�ʾ����Ŀ����XML�����ļ�

```shell
i18n/en-US.json
i18n/zh-CN.json
i18n/zh-Hant.json
i18n/ja-JP.json
```

## ����T4�ļ�

��ֱ�Ӹ���ʾ��T4�ļ�ʹ�ã����ڸ��������ļ�����C#�ĳ�������Key������ʹ��ʱ����

## ʹ��

��ʼ��

```csharp
I18nManager.Instance.Register(new JsonLangPlugin(), new CultureInfo("zh-CN"), out _);
```

ǰ̨ʹ��

```xml
xmlns:c="https://codewf.com"
xmlns:mainLangs="clr-namespace:Localization.Main"
xmlns:developModuleLanguage="clr-namespace:Localization.DevelopModule"

<SelectableTextBlock Text="{c:I18n {x:Static mainLangs:SettingView.Title}}" />
<SelectableTextBlock Text="{c:I18n {x:Static developModuleLanguage:Title2SlugView.Title}, CultureName=zh-CN}" 
```

��̨ʹ��

```csharp
var titleCurrentCulture = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
var titleZhCN = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "zh-CN");
var titleEnUS = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "en-US");
```