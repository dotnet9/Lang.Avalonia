using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace Lang.Avalonia.MarkupExtensions;

/// <summary>
/// 为标记扩展封装一个内部 MultiBinding，并限制转换器只能由派生类初始化一次。
/// </summary>
public abstract class MultiBindingExtensionBase : MarkupExtension
{
    private readonly MultiBinding binding = new();
    private bool converterInitialized;

    /// <summary>
    /// 子绑定集合。
    /// </summary>
    internal IList<BindingBase> Bindings => binding.Bindings;

    /// <summary>
    /// 绑定模式。
    /// </summary>
    protected BindingMode Mode
    {
        get => binding.Mode;
        set => binding.Mode = value;
    }

    /// <summary>
    /// 多值绑定转换器。派生类初始化后不允许外部再次覆盖。
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

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) => binding;
}
