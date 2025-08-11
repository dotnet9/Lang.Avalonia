using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Lang.Avalonia;

public interface ILangPlugin
{
    void Load(CultureInfo cultureInfo);
    void AddResource(params Assembly[] assemblies);
    public CultureInfo Culture { get; set; }
    List<LocalizationLanguage>? GetLanguages();
    string? GetResource(string key, string? cultureName = null);
}