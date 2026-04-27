using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lang.Avalonia.Json.Demo.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly DispatcherTimer _clockTimer;
    private DateTime _currentTime;
    private LocalizationLanguage? _selectLanguage;

    public MainViewModel()
    {
        Languages = I18nManager.Instance.GetLanguages() ?? [];
        SelectLanguage = Languages.FirstOrDefault(l => l.CultureName == I18nManager.Instance.Culture?.Name);

        CurrentTime = DateTime.Now;
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => CurrentTime = DateTime.Now;
        _clockTimer.Start();
    }

    public List<LocalizationLanguage> Languages { get; }

    public string ProviderName { get; } = "JSON";

    public string ResourceStorage { get; } = "I18n/*.json copied to the output directory";

    public string ResourcePipeline { get; } = "Directory scan -> JSON parse -> culture cache";

    public string SampleKey { get; } = Localization.Main.MainView.Title;

    public string PlatformName { get; } = RuntimeInformation.OSDescription.Trim();

    public string ProcessArchitecture { get; } = RuntimeInformation.ProcessArchitecture.ToString();

    public string CurrentCultureName => I18nManager.Instance.Culture?.Name ?? CultureInfo.CurrentUICulture.Name;

    public string SelectedLanguageDescription =>
        SelectLanguage == null ? string.Empty : $"{SelectLanguage.Language} / {SelectLanguage.CultureName}";

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
}
