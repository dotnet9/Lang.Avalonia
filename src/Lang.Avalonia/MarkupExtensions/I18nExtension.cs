using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace Lang.Avalonia.MarkupExtensions;

/// <summary>
/// XAML markup extension for localized resource lookup.
/// </summary>
public class I18nExtension : MarkupExtension
{
    /// <summary>
    /// Creates the markup extension from a resource key.
    /// </summary>
    public I18nExtension(object key)
    {
        Key = key;
    }

    /// <summary>
    /// Creates the markup extension with one format argument.
    /// </summary>
    public I18nExtension(object key, object arg0) : this(key)
    {
        Args.Add(arg0);
    }

    /// <summary>
    /// Creates the markup extension with two format arguments.
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
    }

    /// <summary>
    /// Creates the markup extension with three format arguments.
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
    }

    /// <summary>
    /// Creates the markup extension with four format arguments.
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
    }

    /// <summary>
    /// Creates the markup extension with five format arguments.
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
    /// Creates the markup extension with six format arguments.
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
    /// Creates the markup extension with seven format arguments.
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
    /// Creates the markup extension with eight format arguments.
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
    /// Resource key, constant value, enum value, or binding.
    /// </summary>
    public object Key { get; }

    /// <summary>
    /// Format arguments. Arguments can be constants or Avalonia bindings.
    /// </summary>
    public List<object> Args { get; } = new();

    /// <summary>
    /// Fixed culture name. When unset, the binding follows I18nManager.Culture.
    /// </summary>
    public string? CultureName { get; set; }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new I18nBinding(Key, CultureName, Args).ProvideValue(serviceProvider);
}
