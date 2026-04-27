using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Lang.Avalonia.Converters;

/// <summary>
/// 将 XAML 中传入的资源 Key 规范化为字符串。
/// </summary>
public class I18nKeyConverter : IValueConverter
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
    /// 获取资源 Key。枚举值会转换为“枚举类型_枚举项”的约定名称。
    /// </summary>
    public static string? Get(object? value)
    {
        return value switch
        {
            Enum v => $"{value.GetType().Name}_{v}",
            string key => key,
            _ => string.Empty
        };
    }
}
