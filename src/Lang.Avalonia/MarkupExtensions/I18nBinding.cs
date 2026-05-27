using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Lang.Avalonia.Converters;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Lang.Avalonia.MarkupExtensions;

/// <summary>
/// MultiBinding implementation used by I18nExtension.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class I18nBinding : MultiBindingExtensionBase
{
    /// <summary>
    /// Creates a localization binding from a resource key or key binding.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "I18nBinding intentionally accepts runtime Avalonia bindings for dynamic localization keys.")]
    public I18nBinding(object key)
    {
        Mode = BindingMode.OneWay;
        Converter = new I18nConverter();
        ConverterParameter = this;
        KeyConverter = new I18nKeyConverter();

        var cultureBinding = new CompiledBindingExtension
        {
            Source = I18nManager.Instance,
            Mode = BindingMode.OneWay,
            Path = new CompiledBindingPathBuilder()
                .Property(
                    new ClrPropertyInfo(
                        nameof(I18nManager.Culture),
                        target => ((I18nManager)target).Culture,
                        (target, value) => ((I18nManager)target).Culture = value as CultureInfo,
                        typeof(CultureInfo)),
                    PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build()
        };

        Bindings.Add(cultureBinding);

        Key = key;
        if (Key is not BindingBase keyBinding)
        {
            keyBinding = new CompiledBindingExtension { Source = key };
        }

        Bindings.Add(keyBinding);
    }

    /// <summary>
    /// Creates a localization binding with a fixed culture and format arguments.
    /// </summary>
    public I18nBinding(object key, string? cultureName, IReadOnlyCollection<object> args) : this(key)
    {
        CultureName = cultureName;

        foreach (var arg in args)
        {
            if (arg is BindingBase binding)
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

    /// <summary>
    /// Resource key, constant value, enum value, or Avalonia binding.
    /// </summary>
    public object Key { get; }

    /// <summary>
    /// Fixed culture name. When unset, the binding follows I18nManager.Culture.
    /// </summary>
    public string? CultureName { get; set; }

    /// <summary>
    /// Argument indexes. Binding arguments point to MultiBinding indexes; constants point to Args indexes.
    /// </summary>
    internal List<(bool IsBinding, int Index)> Indexes { get; } = [];

    /// <summary>
    /// Constant arguments passed to string.Format.
    /// </summary>
    internal List<object> Args { get; } = [];

    /// <summary>
    /// Converts the resource key to its lookup string.
    /// </summary>
    public IValueConverter? KeyConverter { get; set; }

    /// <summary>
    /// Optional final value converter.
    /// </summary>
    public IValueConverter? ValueConverter { get; set; }
}
