using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Lang.Avalonia;

public interface ILangPlugin
{
    internal Dictionary<string, LocalizationLanguage> Resources { get; }

    void Load(CultureInfo cultureInfo);

    void AddResource(params IEnumerable<Assembly> assemblies);

    CultureInfo Culture { get; set; }

    IReadOnlyCollection<LocalizationLanguage> GetLanguages();

    string GetResource(string key, string? cultureName = null);

    [MemberNotNull(nameof(Culture))]
    internal void EnsureLoaded()
    {
        if (Culture is null)
            throw new InvalidOperationException($"Please call {nameof(Load)} method before using the plugin.");
    }

    internal bool GetResourceInternal(string culture, string key, [NotNullWhen(true)] out string? result)
    {
        result = null;
        return Resources.TryGetValue(culture, out var currentLanguages)
               && currentLanguages.Languages.TryGetValue(key, out result);
    }
}
