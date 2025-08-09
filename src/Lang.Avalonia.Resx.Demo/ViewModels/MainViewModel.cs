using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Lang.Avalonia.Resx.Demo.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        Languages = new List<LocalizationLanguage>()
        {
            new(){ CultureName = "en-US", Description = "English", Language = "English"},
            new(){ CultureName = "zh-CN", Description = "Chinese (Simplified)", Language = "Chinese (Simplified)"},
            new(){ CultureName = "zh-Hant", Description = "Chinese (Traditional)", Language = "Chinese (Traditional)"},
            new(){ CultureName = "ja-JP", Description = "Japanese", Language = "Japanese"}

        };
        SelectLanguage = Languages?.FirstOrDefault(l => l.CultureName == I18nManager.Instance.Culture.Name);

        var titleCurrentCulture = I18nManager.Instance.GetResource(Localization.Main.MainView.Title);
        var titleZhCN = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "zh-CN");
        var titleEnUS = I18nManager.Instance.GetResource(Localization.Main.MainView.Title, "en-US");

        Task.Run(async () =>
        {
            while (true)
            {
                CurrentTime = DateTime.Now;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        });
    }

    public List<LocalizationLanguage>? Languages { get; set; }
    public LocalizationLanguage? _selectLanguage;

    public LocalizationLanguage? SelectLanguage
    {
        get => _selectLanguage;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectLanguage, value);
            I18nManager.Instance.Culture = new CultureInfo(value.CultureName);
        }
    }

    private DateTime _currentTime;

    public DateTime CurrentTime
    {
        get => _currentTime;
        set => this.RaiseAndSetIfChanged(ref _currentTime, value);
    }
}