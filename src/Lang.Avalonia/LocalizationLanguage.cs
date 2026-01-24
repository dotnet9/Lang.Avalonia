using System.Collections.Generic;

namespace Lang.Avalonia;

public class LocalizationLanguage
{
    public string Language { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string CultureName { get; set; } = null!;

    public Dictionary<string, string> Languages { get; } = new();
}
