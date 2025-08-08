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
    public Dictionary<string, string> Resources { get; } = new();
    public string Mark { get; set; } = "i18n";
    private Dictionary<Type, ResourceManager>? _resourceManagers;


    public CultureInfo Culture
    {
        get;
        set
        {
            field = value;
            Sync();
        }
    }

    public void Load()
    {
        Culture = CultureInfo.InvariantCulture;
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
        Sync();
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

        Sync();
    }

    public List<LocalizationLanguage>? GetLanguages() =>
        throw new NotSupportedException("This plugin does not support the current interface for the time being.");

    public string? GetResource(string key, string? cultureName = null)
    {
        var culture = Culture.Name;
        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            culture = cultureName;
        }


        if (Resources.TryGetValue(key, out var resource) && resource is string result)
        {
            return result;
        }

        return key;
    }


    private void Sync()
    {
        if (_resourceManagers == null || _resourceManagers.Count == 0)
        {
            return;
        }

        IEnumerable<DictionaryEntry> GetResources(ResourceManager resourceManager)
        {
            var baseEntries = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)
                ?.OfType<DictionaryEntry>();
            var cultureEntries = resourceManager.GetResourceSet(Culture, true, true)?.OfType<DictionaryEntry>();
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

        Resources.Clear();
        foreach (var pair in _resourceManagers)
        {
            pair.Key.GetProperty("Culture", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, Culture);
            foreach (var entry in GetResources(pair.Value))
            {
                if (entry.Value is string value)
                {
                    Resources[entry.Key.ToString()] = value;
                }
            }
        }
    }
}