using System.Globalization;
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lang.Avalonia.Json;

namespace Lang.Avalonia.Json.BrowserDemo;

public partial class App : Application
{
    /// <summary>
    /// true = 注册前把嵌入资源加入插件（浏览器可用，默认）；false = 默认输出目录扫描模式（复现 issue #9）。
    /// </summary>
    public static bool UseEmbeddedResources { get; set; } = true;

    /// <summary>
    /// 当前注册到 I18nManager 的插件实例，供诊断面板读取。
    /// </summary>
    public static JsonLangPlugin? Plugin { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var plugin = new JsonLangPlugin();
        Plugin = plugin;

        if (UseEmbeddedResources)
        {
            plugin.AddResource(typeof(App).Assembly);
        }

        I18nManager.Instance.Register(plugin, new CultureInfo("zh-CN"));

        if (ApplicationLifetime is IActivityApplicationLifetime activityLifetime)
        {
            // Avalonia 12 浏览器生命周期：通过工厂提供主视图
            activityLifetime.MainViewFactory = () => new MainView();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
