using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lang.Avalonia.Xml.Demo.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly DispatcherTimer _clockTimer;
    private DateTime _currentTime;
    private LocalizationLanguage? _selectLanguage;

    public MainViewModel()
    {
        Languages = CreateLanguages(I18nManager.Instance.GetLanguages()?.Select(language => language.CultureName));
        SelectLanguage = Languages.FirstOrDefault(l => l.CultureName == I18nManager.Instance.Culture?.Name)
            ?? Languages.FirstOrDefault();

        CurrentTime = DateTime.Now;
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => CurrentTime = DateTime.Now;
        _clockTimer.Start();
    }

    public List<LocalizationLanguage> Languages { get; }

    public string ProviderName { get; } = "Runtime XML Files";

    public string ResourceStorage { get; } = "Loose XML language packs deployed with the application.";

    public string ResourcePipeline { get; } = "File scan -> XML parse -> key dictionary -> culture fallback.";

    public string SampleKey { get; } = Localization.Main.MainView.Title;

    public string PlatformName { get; } = RuntimeInformation.OSDescription.Trim();

    public string ProcessArchitecture { get; } = RuntimeInformation.ProcessArchitecture.ToString();

    public string CurrentCultureName => I18nManager.Instance.Culture?.Name ?? CultureInfo.CurrentUICulture.Name;

    public string SelectedLanguageDescription =>
        SelectLanguage == null ? string.Empty : $"{SelectLanguage.CultureName} · {SelectLanguage.DetailText}";

    public LocalizationLanguage? SelectLanguage
    {
        get => _selectLanguage;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectLanguage, value);
            if (value == null)
            {
                return;
            }

            I18nManager.Instance.Culture = new CultureInfo(value.CultureName);
            this.RaisePropertyChanged(nameof(CurrentCultureName));
            this.RaisePropertyChanged(nameof(SelectedLanguageDescription));
        }
    }

    public DateTime CurrentTime
    {
        get => _currentTime;
        set => this.RaiseAndSetIfChanged(ref _currentTime, value);
    }

    private static List<LocalizationLanguage> CreateLanguages(IEnumerable<string>? cultureNames)
    {
        return (cultureNames ?? [])
            .Where(cultureName => !string.IsNullOrWhiteSpace(cultureName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(CreateLanguage)
            .OrderBy(GetSortOrder)
            .ThenBy(language => language.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static LocalizationLanguage CreateLanguage(string cultureName)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            return new LocalizationLanguage
            {
                CultureName = culture.Name,
                Language = culture.EnglishName,
                Description = culture.NativeName
            };
        }
        catch (CultureNotFoundException)
        {
            return new LocalizationLanguage
            {
                CultureName = cultureName,
                Language = cultureName,
                Description = cultureName
            };
        }
    }

    private static int GetSortOrder(LocalizationLanguage language) => language.CultureName switch
    {
        "zh-CN" => 0,
        "zh-Hant" => 1,
        "en-US" => 2,
        "ja-JP" => 3,
        _ => 4
    };
}
