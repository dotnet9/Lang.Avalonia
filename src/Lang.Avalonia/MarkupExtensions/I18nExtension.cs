using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Markup.Xaml;

namespace Lang.Avalonia.MarkupExtensions;


[RequiresUnreferencedCode(nameof(I18nBinding))]
public class I18nExtension(object key) : MarkupExtension
{
    public I18nExtension(object key, object arg0) : this(key)
    {
        Args.Add(arg0);
    }

    public I18nExtension(object key, object arg0, object arg1) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
    }

    public I18nExtension(object key, object arg0, object arg1, object arg2) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
    }

    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
    }

    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3, object arg4) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
        Args.Add(arg4);
    }

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

    public object Key { get; } = key;

    public List<object> Args { get; } = [];
    
    public string? CultureName { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => new I18nBinding(Key, CultureName, Args);
}
