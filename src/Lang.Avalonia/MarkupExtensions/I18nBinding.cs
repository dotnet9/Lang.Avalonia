using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Lang.Avalonia.Converters;

namespace Lang.Avalonia.MarkupExtensions;

[EditorBrowsable(EditorBrowsableState.Never)]
public class I18nBinding : MultiBindingExtensionBase
{
    public I18nBinding(object key)
    {
        Mode = BindingMode.OneWay;
        Converter = new I18nConverter();
        ConverterParameter = this;
        KeyConverter = new I18nKeyConverter();
        Key = key;

        var cultureBinding = new CompiledBindingExtension
        {
            Source = I18nManager.Instance,
            Mode = BindingMode.OneWay,
            Path = new CompiledBindingPathBuilder()
                .Property(
                    new ClrPropertyInfo(
                        nameof(I18nManager.Culture),
                        target => ((I18nManager) target).Culture,
                        (target, value) => ((I18nManager) target).Culture = new CultureInfo((string) (value ?? throw new ArgumentNullException(nameof(value)))),
                        typeof(CultureInfo)),
                    PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build()
        };

        Bindings.Add(cultureBinding);

        if (key is not BindingBase keyBinding)
            keyBinding = new CompiledBindingExtension { Source = key };

        Bindings.Add(keyBinding);
    }

    public I18nBinding(object key, string? cultureName, IReadOnlyCollection<object> args) : this(key)
    {
        CultureName = cultureName;

        foreach (var arg in args)
        {
            if (arg is IBinding binding)
            {
                Indexes.Add((true, Bindings.Count));
                Bindings.Add(binding);
            }
            else
            {
                Indexes.Add((false, Args.Count));
                Args.Add(arg);
            }
        }
    }

    internal List<(bool IsBinding, int Index)> Indexes { get; } = [];

    public object Key { get; }

    public string? CultureName { get; set; }

    internal List<object> Args { get; } = [];

    public IValueConverter? KeyConverter { get; set; }

    public IValueConverter? ValueConverter { get; set; }
}
