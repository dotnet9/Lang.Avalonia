using Avalonia.Data;
using Avalonia.Data.Converters;
using System;

namespace Lang.Avalonia.MarkupExtensions;

/// <summary>
/// 为标记扩展封装只允许内部设置的 MultiBinding 转换器。
/// </summary>
public abstract class MultiBindingExtensionBase : MultiBinding
{
    /// <summary>
    /// 多值绑定转换器。派生类初始化后不允许外部再次覆盖。
    /// </summary>
    public new IMultiValueConverter? Converter
    {
        get => base.Converter;
        protected set
        {
            if (base.Converter != null)
            {
                throw new InvalidOperationException($"The {GetType().Name}.Converter property is readonly.");
            }

            base.Converter = value;
        }
    }
}
