using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Lang.Avalonia.Resx;

public class ResxLangPlugin : ILangPlugin
{
    public Dictionary<string, LocalizationLanguage> Resources { get; } = new();
    public string Mark { get; set; } = "i18n";
    private Dictionary<Type, ResourceManager>? _resourceManagers;

    private CultureInfo _defaultCulture;

    public CultureInfo Culture
    {
        get;
        set
        {
            field = value;
            Sync(value);
        }
    }

    public void Load(CultureInfo cultureInfo)
    {
        _defaultCulture = cultureInfo;
        Culture = cultureInfo;
        _resourceManagers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly =>
                assembly.GetTypes()
                    .Where(type => type.FullName.Contains(Mark, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(
                        type => type,
                        type => type.GetProperty(nameof(ResourceManager),
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                            ?.GetValue(null, null) as ResourceManager)
            )
            .Where(pair => pair.Value != null)
            .ToDictionary(pair => pair.Key, pair => pair.Value!);
        Sync(Culture);
    }

    public void AddResource(params Assembly[] assemblies)
    {
        var dicts = assemblies.SelectMany(assembly =>
                assembly.GetTypes()
                    .Where(type => type.FullName.Contains(Mark, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(
                        type => type,
                        type => type.GetProperty(nameof(ResourceManager),
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                            ?.GetValue(null, null) as ResourceManager)
            )
            .Where(pair => pair.Value != null)
            .ToDictionary(pair => pair.Key, pair => pair.Value!);
        if (dicts.Count != 0)
        {
            foreach (KeyValuePair<Type, ResourceManager> pair in dicts)
            {
                if (!_resourceManagers.ContainsKey(pair.Key))
                {
                    _resourceManagers.Add(pair.Key, pair.Value);
                }
            }
        }

        Sync(Culture);
    }

    public List<LocalizationLanguage>? GetLanguages() =>
        throw new NotSupportedException("This plugin does not support the current interface for the time being.");

    public string? GetResource(string key, string? cultureName = null)
    {
        var culture = Culture.Name;

        string? GetResource()
        {
            if (Resources.TryGetValue(culture, out var currentLanguages)
                && currentLanguages.Languages.TryGetValue(key, out string resource))
            {
                return resource;
            }

            return default;
        }

        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            culture = cultureName;
        }

        bool isFirst = true;
        var resource = GetResource();
        if (!string.IsNullOrWhiteSpace(resource))
        {
            return resource;
        }

        Sync(new CultureInfo(culture));
        resource = GetResource();
        if (!string.IsNullOrWhiteSpace(resource))
        {
            return resource;
        }

        culture = _defaultCulture.Name;
        resource = GetResource();
        if (!string.IsNullOrWhiteSpace(resource))
        {
            return resource;
        }

        return key;
    }


    private void Sync(CultureInfo cultureInfo)
    {
        if (_resourceManagers == null || _resourceManagers.Count == 0)
        {
            return;
        }

        IEnumerable<DictionaryEntry> GetResources(ResourceManager resourceManager)
        {
            var baseEntries = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)
                ?.OfType<DictionaryEntry>();
            var cultureEntries = resourceManager.GetResourceSet(cultureInfo, true, true)?.OfType<DictionaryEntry>();
            if (cultureEntries == null || baseEntries == null)
            {
                yield break;
            }

            foreach (var entry in cultureEntries
                         .Concat(baseEntries)
                         .GroupBy(entry => entry.Key)
                         .Select(entries => entries.First()))
            {
                yield return entry;
            }
        }

        var cultureName = cultureInfo.Name;
        LocalizationLanguage? currentLanResources;
        if (Resources.ContainsKey(cultureName))
        {
            currentLanResources = Resources[cultureName];
        }
        else
        {
            currentLanResources = new LocalizationLanguage()
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
            foreach (var entry in GetResources(pair.Value))
            {
                if (entry.Key is string key && entry.Value is string value)
                {
                    currentLanResources.Languages[key] = value;
                }
            }
        }
    }
}