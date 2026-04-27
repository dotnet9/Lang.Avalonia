using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Lang.Avalonia.Converters;

/// <summary>
/// 对本地化结果做最后的值转换。
/// </summary>
public class I18nValueConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Get(value);
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// 返回最终显示值，可按需去除字符串末尾空白。
    /// </summary>
    public static object? Get(object? value, bool trimEnd = false)
    {
        return value switch
        {
            string @string => trimEnd ? @string.TrimEnd() : @string,
            _ => value
        };
    }
}
