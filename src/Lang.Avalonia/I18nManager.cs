using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Lang.Avalonia;

public class I18nManager : INotifyPropertyChanged
{
    private ILangPlugin? _langPlugin;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static I18nManager Instance { get; } = new();

    // 加载指定目录下的所有语言文件（XML格式）
    private I18nManager()
    {
    }

    public bool Register(ILangPlugin plugin, out string? error)
    {
        error = null;
        try
        {
            _langPlugin = plugin;
            plugin.Load();
            Culture = CultureInfo.InvariantCulture;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.ToString();
            return false;
        }
    }


    public CultureInfo? Culture
    {
        get => _langPlugin?.Culture;
        set
        {
            if (_langPlugin == null || Equals(_langPlugin?.Culture, value))
            {
                return;
            }

            _langPlugin!.Culture = value;
            Thread.CurrentThread.CurrentCulture = value;
            Thread.CurrentThread.CurrentUICulture = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public List<LocalizationLanguage>? GetLanguages() => _langPlugin?.GetLanguages();


    public string? GetResource(string key, string? cultureName = null) => _langPlugin?.GetResource(key, cultureName);

    public event EventHandler<EventArgs>? CultureChanged;
}