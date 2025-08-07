using System.Collections.Generic;

namespace Lang.Avalonia;

public interface ILangPlugin
{
    public string ResourcePath { get; set; }
    Dictionary<string, LocalizationLanguage> LoadLanguages();
}