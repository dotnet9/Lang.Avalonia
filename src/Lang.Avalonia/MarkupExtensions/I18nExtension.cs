using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace Lang.Avalonia.MarkupExtensions;

/// <summary>
/// XAML 标记扩展入口，用于将资源 Key 绑定到当前文化下的本地化文本。
/// </summary>
public class I18nExtension : MarkupExtension
{
    /// <summary>
    /// 使用资源 Key 创建标记扩展。
    /// </summary>
    public I18nExtension(object key)
    {
        Key = key;
    }

    /// <summary>
    /// 使用资源 Key 和 1 个格式化参数创建标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0) : this(key)
    {
        Args.Add(arg0);
    }

    /// <summary>
    /// 使用资源 Key 和 2 个格式化参数创建标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
    }

    /// <summary>
    /// 使用资源 Key 和 3 个格式化参数创建标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
    }

    /// <summary>
    /// 使用资源 Key 和 4 个格式化参数创建标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
    }

    /// <summary>
    /// 使用资源 Key 和 5 个格式化参数创建标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3, object arg4) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
        Args.Add(arg4);
    }

    /// <summary>
    /// 使用资源 Key 和 6 个格式化参数创建标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5) :
        this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
        Args.Add(arg4);
        Args.Add(arg5);
    }

    /// <summary>
    /// 使用资源 Key 和 7 个格式化参数创建标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5,
        object arg6) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
        Args.Add(arg4);
        Args.Add(arg5);
        Args.Add(arg6);
    }

    /// <summary>
    /// 使用资源 Key 和 8 个格式化参数创建标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5,
        object arg6, object arg7) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
        Args.Add(arg4);
        Args.Add(arg5);
        Args.Add(arg6);
        Args.Add(arg7);
    }

    /// <summary>
    /// 资源 Key，可为常量、枚举或 Binding。
    /// </summary>
    public object Key { get; }

    /// <summary>
    /// 格式化参数。参数可以是常量，也可以是 Avalonia Binding。
    /// </summary>
    public List<object> Args { get; } = new();

    /// <summary>
    /// 固定使用指定文化；为空时跟随 <see cref="I18nManager.Culture"/>。
    /// </summary>
    public string? CultureName { get; set; }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new I18nBinding(Key, CultureName, Args).ProvideValue(serviceProvider);
}
