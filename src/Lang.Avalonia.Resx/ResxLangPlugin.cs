using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Lang.Avalonia.Resx;

[RequiresUnreferencedCode("Type.GetProperty")]
public class ResxLangPlugin : ILangPlugin
{
    public Dictionary<string, LocalizationLanguage> Resources { get; } = new();

    public string Mark { get; set; } = "i18n";

    private Dictionary<Type, ResourceManager>? _resourceManagers;

    private CultureInfo _defaultCulture = null!;

    public CultureInfo Culture
    {
        get;
        set
        {
            field = value;
            Sync(value);
        }
    } = null!;

    public void Load(CultureInfo cultureInfo)
    {
        _defaultCulture = cultureInfo;
        Culture = cultureInfo;
        _resourceManagers = GetFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        Sync(Culture);
    }

    public void AddResource(params IEnumerable<Assembly> assemblies)
    {
        ((ILangPlugin) this).EnsureLoaded();

        var dict = GetFromAssemblies(assemblies);
        foreach (var pair in dict)
            _resourceManagers!.TryAdd(pair.Key, pair.Value);

        Sync(Culture);
    }

    private Dictionary<Type, ResourceManager> GetFromAssemblies(IEnumerable<Assembly> assemblies) =>
        assemblies.SelectMany(assembly =>
                assembly.GetTypes()
                    .Where(type => type.FullName?.Contains(Mark, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(type => (Type: type, Property: type.GetProperty(nameof(ResourceManager),
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                ?.GetValue(null, null) as ResourceManager))
            .Where(pair => pair.Property is not null)
            .ToDictionary(pair => pair.Type, pair => pair.Property!);

    public IReadOnlyCollection<LocalizationLanguage> GetLanguages() =>
        throw new NotSupportedException("This plugin does not support the current interface for the time being.");

    public string GetResource(string key, string? cultureName = null)
    {
        ((ILangPlugin) this).EnsureLoaded();

        if (string.IsNullOrWhiteSpace(cultureName))
            cultureName = Culture.Name;

        if (((ILangPlugin) this).GetResourceInternal(cultureName, key, out var resource))
            return resource;

        Sync(new CultureInfo(cultureName));

        if (((ILangPlugin) this).GetResourceInternal(cultureName, key, out resource))
            return resource;

        cultureName = _defaultCulture.Name;

        if (((ILangPlugin) this).GetResourceInternal(cultureName, key, out resource))
            return resource;

        return key;
    }

    private void Sync(CultureInfo cultureInfo)
    {
        if (_resourceManagers is not { Count: > 0 })
            return;

        var cultureName = cultureInfo.Name;
        if (!Resources.TryGetValue(cultureName, out var currentLanResources))
        {
            currentLanResources = new()
            {
                Language = cultureInfo.DisplayName,
                Description = cultureInfo.DisplayName,
                CultureName = cultureName
            };
            Resources[cultureName] = currentLanResources;
        }

        foreach (var pair in _resourceManagers)
        {
            pair.Key.GetProperty("Culture", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, cultureInfo);
            foreach (var entry in GetResourcesInternal(pair.Value))
            {
                if (entry is { Key: string key, Value: string value })
                {
                    currentLanResources.Languages[key] = value;
                }
            }
        }

        return;

        IEnumerable<DictionaryEntry> GetResourcesInternal(ResourceManager resourceManager)
        {
            var baseEntries = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)
                ?.OfType<DictionaryEntry>();
            var cultureEntries = resourceManager.GetResourceSet(cultureInfo, true, true)?.OfType<DictionaryEntry>();
            if (cultureEntries is null || baseEntries is null)
                return [];

            return cultureEntries
                .Concat(baseEntries)
                .DistinctBy(entry => entry.Key);
        }
    }
}
