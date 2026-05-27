using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Lang.Avalonia.MarkupExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Lang.Avalonia.Converters;

/// <summary>
/// Resolves the current culture, resource key, and format arguments to the final value.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class I18nConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2
            || values[0] is not CultureInfo
            || parameter is not I18nBinding owner
            || IsUnsetValue(values[1]))
        {
            return BindingOperations.DoNothing;
        }

        var value = values[1];
        if (owner.KeyConverter is { } keyConverter)
        {
            value = keyConverter.Convert(value, typeof(string), null, culture);
        }

        if (value is not string key)
        {
            return value;
        }

        value = I18nManager.Instance.GetResource(key, owner.CultureName);

        if (value is string format && owner.Indexes.Count > 0)
        {
            if (owner.Indexes.Any(item => item.IsBinding && item.Index >= values.Count))
            {
                return BindingOperations.DoNothing;
            }

            var args = owner.Indexes
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

        return owner.ValueConverter is { } valueConverter
            ? valueConverter.Convert(value, targetType, null, culture)
            : value;
    }

    private static bool IsUnsetValue(object? value)
    {
        return ReferenceEquals(value, AvaloniaProperty.UnsetValue)
            || ReferenceEquals(value, BindingOperations.DoNothing);
    }
}
