using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace Lang.Avalonia;

/// <summary>
/// 多语言运行时管理器，负责注册资源插件、切换文化并通知 Avalonia 绑定刷新。
/// </summary>
public class I18nManager : INotifyPropertyChanged
{
    private ILangPlugin? _langPlugin;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 全局多语言管理器实例。
    /// </summary>
    public static I18nManager Instance { get; } = new();

    private I18nManager()
    {
    }

    /// <summary>
    /// 注册语言插件并加载默认文化资源。
    /// </summary>
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

    /// <summary>
    /// 从额外程序集追加资源，常用于模块化应用延迟加载语言包。
    /// </summary>
    public void AddResource(params Assembly[] assemblies)
    {
        _langPlugin?.AddResource(assemblies);
    }

    /// <summary>
    /// 当前文化。设置后会同步当前线程、默认线程文化，并触发绑定刷新。
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
    /// 获取当前插件提供的语言列表。
    /// </summary>
    public List<LocalizationLanguage>? GetLanguages() => _langPlugin?.GetLanguages();

    /// <summary>
    /// 按 Key 获取本地化文本。
    /// </summary>
    public string GetResource(string key, string? cultureName = null) => _langPlugin?.GetResource(key, cultureName) ?? key;

    /// <summary>
    /// 当前文化变化时触发。
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
