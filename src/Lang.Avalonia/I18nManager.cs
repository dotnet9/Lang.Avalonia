using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace Lang.Avalonia;

/// <summary>
/// Runtime localization manager that registers plugins, switches culture, and refreshes bindings.
/// </summary>
public class I18nManager : INotifyPropertyChanged
{
    private ILangPlugin? _langPlugin;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Global localization manager instance.
    /// </summary>
    public static I18nManager Instance { get; } = new();

    private I18nManager()
    {
    }

    /// <summary>
    /// Registers a plugin and loads resources for the default culture.
    /// </summary>
    public void Register(ILangPlugin plugin, CultureInfo? defaultCulture = null)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        var culture = defaultCulture ?? CultureInfo.CurrentUICulture;
        if (!Register(plugin, culture, out var error))
        {
            throw new InvalidOperationException(error);
        }
    }

    /// <summary>
    /// Registers a plugin and loads resources for the default culture.
    /// </summary>
    public bool Register(ILangPlugin plugin, CultureInfo defaultCulture, out string? error)
    {
        ArgumentNullException.ThrowIfNull(plugin);
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

    /// <summary>
    /// Adds resources from extra assemblies.
    /// </summary>
    public void AddResource(params Assembly[] assemblies)
    {
        _langPlugin?.AddResource(assemblies);
    }

    /// <summary>
    /// Current culture. Setting it updates thread cultures and refreshes bindings.
    /// </summary>
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

    /// <summary>
    /// Gets the languages known by the current plugin.
    /// </summary>
    public List<LocalizationLanguage>? GetLanguages() => _langPlugin?.GetLanguages();

    /// <summary>
    /// Gets localized text by key.
    /// </summary>
    public string GetResource(string key, string? cultureName = null) => _langPlugin?.GetResource(key, cultureName) ?? key;

    /// <summary>
    /// Raised when the current culture changes.
    /// </summary>
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
