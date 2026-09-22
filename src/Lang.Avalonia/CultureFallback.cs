using System.Collections.Generic;
using System.Globalization;

namespace Lang.Avalonia;

internal static class CultureFallback
{
    public static bool TryCreateCulture(string? cultureName, out CultureInfo culture)
    {
        try
        {
            culture = new CultureInfo(cultureName ?? string.Empty);
            return true;
        }
        catch (CultureNotFoundException)
        {
            culture = CultureInfo.InvariantCulture;
            return false;
        }
    }

    public static IEnumerable<CultureInfo> Enumerate(CultureInfo culture, CultureInfo defaultCulture)
    {
        var visited = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in EnumerateParents(culture))
        {
            if (visited.Add(candidate.Name))
            {
                yield return candidate;
            }
        }

        foreach (var candidate in EnumerateParents(defaultCulture))
        {
            if (visited.Add(candidate.Name))
            {
                yield return candidate;
            }
        }
    }

    private static IEnumerable<CultureInfo> EnumerateParents(CultureInfo culture)
    {
        var current = culture;
        while (true)
        {
            yield return current;
            if (current.Name.Length == 0)
            {
                yield break;
            }

            current = current.Parent;
        }
    }
}
