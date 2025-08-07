using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Lang.Avalonia;

public class I18nManager : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Dictionary<string, LocalizationLanguage> Resources { get; private set; }
    public static I18nManager Instance { get; } = new();

    // 加载指定目录下的所有语言文件（XML格式）
    private I18nManager()
    {
        _culture = CultureInfo.InvariantCulture;
    }

    public bool Register(ILangPlugin plugin, out string? error)
    {
        error = null;
        try
        {
            Resources = plugin.LoadLanguages();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.ToString();
            return false;
        }
    }


    private CultureInfo _culture;

    public CultureInfo Culture
    {
        get => _culture;
        set
        {
            if (Equals(_culture, value))
            {
                return;
            }

            _culture = value;
            Thread.CurrentThread.CurrentCulture = value;
            Thread.CurrentThread.CurrentUICulture = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler<EventArgs>? CultureChanged;

    public List<LocalizationLanguage>? GetLanguages() => Resources.Select(kvp => kvp.Value).ToList();

    public string? GetResource(string key, string? cultureName = null)
    {
        var culture = Culture.Name;
        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            culture = cultureName;
        }

        if (Instance.Resources.TryGetValue(culture, out var currentLanguages)
            && currentLanguages.Languages.TryGetValue(key, out string resource))
        {
            return resource;
        }

        return key;
    }

    public List<CultureInfo> GetAvailableCultures()
    {
        var availableCultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(culture => !CultureInfo.InvariantCulture.Equals(culture))
            //.Except(existingLanguages)
            .OrderBy(culture => culture.DisplayName)
            .ToList();

        return availableCultures;
    }
}