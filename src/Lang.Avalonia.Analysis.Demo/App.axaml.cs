using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lang.Avalonia.Analysis.Demo.ViewModels;
using Lang.Avalonia.Analysis.Demo.Views;
using Lang.Avalonia.Json;

namespace Lang.Avalonia.Analysis.Demo;

public partial class App : Application
{
    public override void Initialize()
    {
        I18nManager.Instance.Register(new JsonLangPlugin(), out _);
        I18nManager.Instance.Culture = new CultureInfo("zh-CN");
        base.Initialize(); // <-- Required
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}