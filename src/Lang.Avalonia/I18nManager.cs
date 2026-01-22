using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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

    public void Register(ILangPlugin plugin, CultureInfo? defaultCulture = null)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        _langPlugin = plugin;
        Culture = defaultCulture ?? CultureInfo.CurrentUICulture;
        plugin.Load(Culture);
    }

    public bool Register(ILangPlugin plugin, CultureInfo defaultCulture, out string? error)
    {
        error = null;
        try
        {
            _langPlugin = plugin;

            plugin.Load(defaultCulture);
            Culture = defaultCulture;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.ToString();
            return false;
        }
    }

    public void AddResource(params IEnumerable<Assembly> assemblies)
    {
        EnsureRegistered();
        ArgumentNullException.ThrowIfNull(assemblies);
        _langPlugin.AddResource(assemblies);
    }

    public CultureInfo Culture
    {
        get
        {
            EnsureRegistered();
            return _langPlugin.Culture;
        }
        set
        {
            EnsureRegistered();
            ArgumentNullException.ThrowIfNull(value);
            if (Equals(_langPlugin.Culture, value))
                return;

            _langPlugin.Culture = value;
            Thread.CurrentThread.CurrentCulture = value;
            Thread.CurrentThread.CurrentUICulture = value;
            PropertyChanged?.Invoke(this, new(nameof(Culture)));
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public IReadOnlyCollection<LocalizationLanguage> GetLanguages()
    {
        EnsureRegistered();
        return _langPlugin.GetLanguages();
    }

    public string GetResource(string key, string? cultureName = null)
    {
        EnsureRegistered();
        return _langPlugin.GetResource(key, cultureName);
    }

    public event EventHandler<EventArgs>? CultureChanged;

    [MemberNotNull(nameof(_langPlugin))]
    private void EnsureRegistered()
    {
        if (_langPlugin is null)
            throw new InvalidOperationException($"{nameof(I18nManager)} is not registered. Please register a language plugin before using it.");
    }
}
