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
    private readonly object _syncRoot = new();

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

        lock (_syncRoot)
        {
            var previousPlugin = _langPlugin;

            try
            {
                plugin.Load(defaultCulture);
                _langPlugin = plugin;
                SetCulture(defaultCulture);
            }
            catch (Exception ex)
            {
                _langPlugin = previousPlugin;
                error = ex.ToString();
                return false;
            }
        }

        NotifyCultureChanged();
        return true;
    }

    /// <summary>
    /// Adds resources from extra assemblies.
    /// </summary>
    public void AddResource(params Assembly[] assemblies)
    {
        lock (_syncRoot)
        {
            if (_langPlugin == null)
            {
                return;
            }

            _langPlugin.AddResource(assemblies);
            ResourceVersion++;
        }

        NotifyResourcesChanged();
    }

    /// <summary>
    /// Current culture. Setting it updates thread cultures and refreshes bindings.
    /// </summary>
    public CultureInfo? Culture
    {
        get
        {
            lock (_syncRoot)
            {
                return _langPlugin?.Culture;
            }
        }
        set
        {
            if (value == null)
            {
                return;
            }

            lock (_syncRoot)
            {
                if (_langPlugin == null || Equals(_langPlugin.Culture, value))
                {
                    return;
                }

                SetCulture(value);
            }

            NotifyCultureChanged();
        }
    }

    /// <summary>
    /// Gets the languages known by the current plugin.
    /// </summary>
    public List<LocalizationLanguage>? GetLanguages()
    {
        lock (_syncRoot)
        {
            return _langPlugin?.GetLanguages();
        }
    }

    /// <summary>
    /// Gets localized text by key.
    /// </summary>
    public string GetResource(string key, string? cultureName = null)
    {
        lock (_syncRoot)
        {
            return _langPlugin?.GetResource(key, cultureName) ?? key;
        }
    }

    /// <summary>
    /// Raised when the current culture changes.
    /// </summary>
    public event EventHandler<EventArgs>? CultureChanged;

    /// <summary>
    /// Monotonically increasing version of the loaded resource set.
    /// </summary>
    public int ResourceVersion
    {
        get
        {
            lock (_syncRoot)
            {
                return _resourceVersion;
            }
        }
        internal set
        {
            lock (_syncRoot)
            {
                _resourceVersion = value;
            }
        }
    }

    private int _resourceVersion;

    /// <summary>
    /// Raised after resources are added to the current plugin.
    /// </summary>
    public event EventHandler<EventArgs>? ResourcesChanged;

    private void SetCulture(CultureInfo culture)
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

    }

    private void NotifyCultureChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NotifyResourcesChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResourceVersion)));
        ResourcesChanged?.Invoke(this, EventArgs.Empty);
    }
}
