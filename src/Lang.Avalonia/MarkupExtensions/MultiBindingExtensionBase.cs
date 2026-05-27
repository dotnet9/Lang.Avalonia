using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace Lang.Avalonia.MarkupExtensions;

/// <summary>
/// Wraps a MultiBinding and lets derived extensions initialize converter state once.
/// </summary>
public abstract class MultiBindingExtensionBase : MarkupExtension
{
    private readonly MultiBinding binding = new();
    private bool converterInitialized;
    private bool converterParameterInitialized;

    /// <summary>
    /// Child bindings used by the wrapped MultiBinding.
    /// </summary>
    internal IList<BindingBase> Bindings => binding.Bindings;

    /// <summary>
    /// Binding mode used by the wrapped MultiBinding.
    /// </summary>
    protected BindingMode Mode
    {
        get => binding.Mode;
        set => binding.Mode = value;
    }

    /// <summary>
    /// Multi-value converter. Derived classes can set it during initialization.
    /// </summary>
    protected IMultiValueConverter? Converter
    {
        get => binding.Converter;
        set
        {
            if (converterInitialized)
            {
                throw new InvalidOperationException($"The {GetType().Name}.Converter property is readonly.");
            }

            binding.Converter = value;
            converterInitialized = true;
        }
    }

    /// <summary>
    /// Converter parameter. Derived classes can set it during initialization.
    /// </summary>
    protected object? ConverterParameter
    {
        get => binding.ConverterParameter;
        set
        {
            if (converterParameterInitialized)
            {
                throw new InvalidOperationException($"The {GetType().Name}.ConverterParameter property is readonly.");
            }

            binding.ConverterParameter = value;
            converterParameterInitialized = true;
        }
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) => binding;
}
