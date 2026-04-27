using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Lang.Avalonia.Resx;

public class ResxLangPlugin : ILangPlugin
{
    private CultureInfo _culture = CultureInfo.InvariantCulture;
    private CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
    private Dictionary<Type, ResourceManager> _resourceManagers = new();

    public Dictionary<string, LocalizationLanguage> Resources { get; } = new();

    public string Mark { get; set; } = "i18n";

    public CultureInfo Culture
    {
        get => _culture;
        set
        {
            _culture = value;
            Sync(value);
        }
    }

    public void Load(CultureInfo cultureInfo)
    {
        _defaultCulture = cultureInfo;
        _resourceManagers = FindResourceManagers(AppDomain.CurrentDomain.GetAssemblies());
        Resources.Clear();
        Culture = cultureInfo;
    }

    public void AddResource(params Assembly[] assemblies)
    {
        var managers = FindResourceManagers(assemblies);
        foreach (var pair in managers)
        {
            _resourceManagers.TryAdd(pair.Key, pair.Value);
        }

        Sync(Culture);
    }

    public List<LocalizationLanguage>? GetLanguages() => Resources.Select(kvp => kvp.Value).ToList();

    public string? GetResource(string key, string? cultureName = null)
    {
        var culture = Culture;
        if (!string.IsNullOrWhiteSpace(cultureName) && !TryCreateCulture(cultureName, out culture))
        {
            return key;
        }

        if (TryGetCachedResource(culture.Name, key, out var resource))
        {
            return resource;
        }

        Sync(culture);
        if (TryGetCachedResource(culture.Name, key, out resource))
        {
            return resource;
        }

        if (TryGetCachedResource(_defaultCulture.Name, key, out resource))
        {
            return resource;
        }

        Sync(_defaultCulture);
        if (TryGetCachedResource(_defaultCulture.Name, key, out resource))
        {
            return resource;
        }

        return key;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "RESX plugin discovers generated resource properties by convention; trimmed apps should root resource designer types.")]
    private void Sync(CultureInfo cultureInfo)
    {
        if (_resourceManagers.Count == 0)
        {
            return;
        }

        var cultureName = cultureInfo.Name;
        if (!Resources.TryGetValue(cultureName, out var currentLanguageResources))
        {
            currentLanguageResources = new LocalizationLanguage
            {
                Language = cultureInfo.DisplayName,
                Description = cultureInfo.DisplayName,
                CultureName = cultureName
            };
            Resources[cultureName] = currentLanguageResources;
        }

        foreach (var pair in _resourceManagers)
        {
            pair.Key.GetProperty("Culture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, cultureInfo);

            foreach (var entry in GetResources(pair.Value, cultureInfo))
            {
                if (entry.Key is string key && entry.Value is string value)
                {
                    currentLanguageResources.Languages[key] = value;
                }
            }
        }
    }

    private bool TryGetCachedResource(string cultureName, string key, out string? resource)
    {
        resource = null;
        return Resources.TryGetValue(cultureName, out var currentLanguages)
            && currentLanguages.Languages.TryGetValue(key, out resource)
            && !string.IsNullOrWhiteSpace(resource);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "RESX plugin discovers generated ResourceManager properties by convention.")]
    private Dictionary<Type, ResourceManager> FindResourceManagers(IEnumerable<Assembly> assemblies)
    {
        var resourceManagers = new Dictionary<Type, ResourceManager>();
        foreach (var type in assemblies
                     .Where(assembly => assembly != null)
                     .Distinct()
                     .SelectMany(GetLoadableTypes)
                     .Where(type => type.FullName?.Contains(Mark, StringComparison.OrdinalIgnoreCase) == true))
        {
            var manager = type.GetProperty(nameof(ResourceManager),
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                ?.GetValue(null, null) as ResourceManager;
            if (manager != null)
            {
                resourceManagers[type] = manager;
            }
        }

        return resourceManagers;
    }

    private static IEnumerable<DictionaryEntry> GetResources(ResourceManager resourceManager, CultureInfo cultureInfo)
    {
        IEnumerable<DictionaryEntry> baseEntries = [];
        IEnumerable<DictionaryEntry> cultureEntries = [];

        try
        {
            baseEntries = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)
                ?.OfType<DictionaryEntry>() ?? [];
            cultureEntries = resourceManager.GetResourceSet(cultureInfo, true, true)
                ?.OfType<DictionaryEntry>() ?? [];
        }
        catch (MissingManifestResourceException)
        {
        }

        return cultureEntries
            .Concat(baseEntries)
            .GroupBy(entry => entry.Key)
            .Select(entries => entries.First());
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "RESX plugin scans loaded assemblies to find generated resource designer types by convention.")]
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null)!;
        }
        catch
        {
            return [];
        }
    }

    private static bool TryCreateCulture(string cultureName, out CultureInfo culture)
    {
        try
        {
            culture = new CultureInfo(cultureName);
            return true;
        }
        catch (CultureNotFoundException)
        {
            culture = CultureInfo.InvariantCulture;
            return false;
        }
    }
}
