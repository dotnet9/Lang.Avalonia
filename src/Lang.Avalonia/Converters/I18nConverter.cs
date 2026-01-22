using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Lang.Avalonia.MarkupExtensions;

namespace Lang.Avalonia.Converters;

[EditorBrowsable(EditorBrowsableState.Never)]
public class I18nConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [CultureInfo, var value, ..] || parameter is not I18nBinding owner)
            return null;

        if (owner.KeyConverter is { } keyConverter)
            value = keyConverter.Convert(value, typeof(string), null, culture);

        if (value is not string key)
            return value;

        value = I18nManager.Instance.GetResource(key, owner.CultureName);

        if (value is string format)
            value = string.Format(format, owner.Indexes
                .Select(item => item.IsBinding ? values[item.Index] : owner.Args[item.Index])
                .ToArray());

        if (owner.ValueConverter is { } valueConverter)
            return valueConverter.Convert(value, typeof(string), null, culture);

        return value;
    }
}
