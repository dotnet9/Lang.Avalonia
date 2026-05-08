using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Lang.Avalonia.MarkupExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Lang.Avalonia.Converters;

/// <summary>
/// 多值绑定转换器：根据当前文化、资源 Key 和格式化参数解析最终文本。
/// </summary>
public class I18nConverter(I18nBinding owner) : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not CultureInfo || IsUnsetValue(values[1]))
        {
            return BindingOperations.DoNothing;
        }

        var value = values[1];
        if (owner.KeyConverter.Convert(value, typeof(string), null, culture) is not string key)
        {
            return value;
        }

        value = I18nManager.Instance.GetResource(key, owner.CultureName);

        if (value is string format && owner.Args.Indexes.Count > 0)
        {
            if (owner.Args.Indexes.Any(item => item.IsBinding && item.Index >= values.Count))
            {
                return BindingOperations.DoNothing;
            }

            var args = owner.Args.Indexes
                .Select(item => item.IsBinding ? values[item.Index] : owner.Args[item.Index])
                .ToArray();

            if (args.Any(IsUnsetValue))
            {
                return BindingOperations.DoNothing;
            }

            try
            {
                value = string.Format(culture, format, args);
            }
            catch (FormatException)
            {
                value = format;
            }
        }

        return owner.ValueConverter.Convert(value, targetType, null, culture);
    }

    private static bool IsUnsetValue(object? value)
    {
        return ReferenceEquals(value, AvaloniaProperty.UnsetValue)
            || ReferenceEquals(value, BindingOperations.DoNothing);
    }
}
