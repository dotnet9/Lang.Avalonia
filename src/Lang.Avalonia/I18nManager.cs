using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace Lang.Avalonia;

public class I18nManager : INotifyPropertyChanged
{
    private ILangPlugin? _langPlugin;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static I18nManager Instance { get; } = new();

    private I18nManager()
    {
    }

    public bool Register(ILangPlugin plugin, CultureInfo defaultCulture, out string? error)
    {
        error = null;
        var previousPlugin = _langPlugin;

        try
        {
            plugin.Load(defaultCulture);
            _langPlugin = plugin;
            SetCulture(defaultCulture, notify: true);
            return true;
        }
        catch (Exception ex)
        {
            _langPlugin = previousPlugin;
            error = ex.ToString();
            return false;
        }
    }

    public void AddResource(params Assembly[] assemblies)
    {
        _langPlugin?.AddResource(assemblies);
    }


    public CultureInfo? Culture
    {
        get => _langPlugin?.Culture;
        set
        {
            if (_langPlugin == null || value == null || Equals(_langPlugin.Culture, value))
            {
                return;
            }

            SetCulture(value, notify: true);
        }
    }

    public List<LocalizationLanguage>? GetLanguages() => _langPlugin?.GetLanguages();


    public string? GetResource(string key, string? cultureName = null) => _langPlugin?.GetResource(key, cultureName);

    public event EventHandler<EventArgs>? CultureChanged;

    private void SetCulture(CultureInfo culture, bool notify)
    {
        if (_langPlugin == null)
        {
            return;
        }

        _langPlugin.Culture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        if (!notify)
        {
            return;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
        CultureChanged?.Invoke(this, EventArgs.Empty);
    }
}
