using Avalonia.Data;
using Avalonia.Data.Converters;
using Lang.Avalonia.Converters;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lang.Avalonia.MarkupExtensions;

/// <summary>
/// 多值绑定实现：监听当前文化、资源 Key 以及格式化参数，并交给转换器生成文本。
/// </summary>
public class I18nBinding : MultiBindingExtensionBase
{
    /// <summary>
    /// 使用资源 Key 创建本地化绑定。
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "I18nBinding intentionally accepts runtime Avalonia bindings for dynamic localization keys.")]
    public I18nBinding(object key)
    {
        Mode = BindingMode.OneWay;
        Converter = new I18nConverter(this);
        KeyConverter = new I18nKeyConverter();
        ValueConverter = new I18nValueConverter();
        Args = new ArgCollection(this);

        var cultureBinding = new Binding { Source = I18nManager.Instance, Path = nameof(I18nManager.Culture) };
        Bindings.Add(cultureBinding);

        Key = key;
        if (Key is not BindingBase keyBinding)
        {
            keyBinding = new Binding { Source = key };
        }

        Bindings.Add(keyBinding);
    }

    /// <summary>
    /// 使用固定文化和格式化参数创建本地化绑定。
    /// </summary>
    public I18nBinding(object key, string? cultureName, List<object> args) : this(key)
    {
        CultureName = cultureName;
        if (args is not { Count: > 0 })
        {
            return;
        }

        foreach (object arg in args)
        {
            Args.Add(arg);
        }
    }

    /// <summary>
    /// 资源 Key，可为常量、枚举或 Avalonia Binding。
    /// </summary>
    public object Key { get; }

    /// <summary>
    /// 固定文化名称。设置后该绑定不会跟随当前线程文化取值。
    /// </summary>
    public string? CultureName { get; set; }

    /// <summary>
    /// 参与 string.Format 的参数集合。
    /// </summary>
    public ArgCollection Args { get; }

    /// <summary>
    /// 资源 Key 转换器。
    /// </summary>
    public IValueConverter KeyConverter { get; set; }

    /// <summary>
    /// 资源值转换器。
    /// </summary>
    public IValueConverter ValueConverter { get; set; }
}
