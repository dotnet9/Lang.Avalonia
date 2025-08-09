using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Lang.Avalonia.Json.Demo.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        Languages = I18nManager.Instance.GetLanguages();
        SelectLanguage = Languages?.FirstOrDefault(l => l.CultureName == I18nManager.Instance.Culture.Name);

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

    public ObservableCollection<CultureInfo> AllCultures { get; }
}