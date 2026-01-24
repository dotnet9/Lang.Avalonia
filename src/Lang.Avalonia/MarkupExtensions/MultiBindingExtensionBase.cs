using Avalonia.Data;
using Avalonia.Data.Converters;
using System;

namespace Lang.Avalonia.MarkupExtensions;

public abstract class MultiBindingExtensionBase : MultiBinding
{
    public new IMultiValueConverter? Converter
    {
        get => base.Converter;
        protected set
        {
            if (base.Converter is null)
                base.Converter = value;
            else
                throw new InvalidOperationException($"The {GetType().Name}.Converter property is readonly.");
        }
    }
}
