using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Lang.Avalonia.Resx;

/// <summary>
/// RESX 语言资源插件。支持显式注册 ResourceManager，也支持按约定发现生成的 ResourceManager，并将资源同步到统一缓存。
/// </summary>
public class ResxLangPlugin : ILangPlugin
{
    private CultureInfo _culture = CultureInfo.InvariantCulture;
    private CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
    private readonly List<ResourceManager> _resourceManagers = new();

    /// <summary>
    /// 创建 RESX 语言资源插件。未显式添加资源时会按 <see cref="Mark"/> 扫描已加载程序集。
    /// </summary>
    public ResxLangPlugin()
    {
    }

    /// <summary>
    /// 使用指定的 <see cref="ResourceManager"/> 创建 RESX 语言资源插件。裁剪发布时优先使用该构造函数。
    /// </summary>
    public ResxLangPlugin(ResourceManager resourceManager)
    {
        AddResource(resourceManager);
    }

    /// <summary>
    /// 使用指定的 RESX Designer 类型创建 RESX 语言资源插件。裁剪发布时该类型的静态属性会被保留。
    /// </summary>
    public ResxLangPlugin([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type resourceType)
    {
        AddResourceType(resourceType);
    }

    /// <summary>
    /// 已加载的语言资源缓存，Key 为文化名称。
    /// </summary>
    public Dictionary<string, LocalizationLanguage> Resources { get; } = new();

    /// <summary>
    /// 用于筛选资源 Designer 类型的命名标记。
    /// </summary>
    public string Mark { get; set; } = "i18n";

    /// <inheritdoc />
    public CultureInfo Culture
    {
        get => _culture;
        set
        {
            _culture = value;
            Sync(value);
        }
    }

    /// <inheritdoc />
    public void Load(CultureInfo cultureInfo)
    {
        _defaultCulture = cultureInfo;
        Resources.Clear();
        if (_resourceManagers.Count == 0)
        {
            AddResourceManagers(FindResourceManagers(AppDomain.CurrentDomain.GetAssemblies()));
        }

        Culture = cultureInfo;
    }

    /// <inheritdoc />
    public void AddResource(params Assembly[] assemblies)
    {
        AddResourceManagers(FindResourceManagers(assemblies));
        Sync(Culture);
    }

    /// <summary>
    /// 从指定 <see cref="ResourceManager"/> 追加 RESX 资源。裁剪发布时优先使用该方法。
    /// </summary>
    public void AddResource(params ResourceManager[] resourceManagers)
    {
        AddResourceManagers(resourceManagers);
        Sync(Culture);
    }

    /// <summary>
    /// 从指定 RESX Designer 类型追加资源。裁剪发布时该类型的静态属性会被保留。
    /// </summary>
    public void AddResourceType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type resourceType)
    {
        if (TryGetResourceManager(resourceType, out var resourceManager))
        {
            AddResource(resourceManager);
        }
    }

    /// <summary>
    /// 从指定 RESX Designer 类型追加资源。裁剪发布时该类型的静态属性会被保留。
    /// </summary>
    public void AddResource<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] TResource>()
    {
        AddResourceType(typeof(TResource));
    }

    /// <inheritdoc />
    public List<LocalizationLanguage>? GetLanguages() => Resources.Select(kvp => kvp.Value).ToList();

    /// <inheritdoc />
    public string GetResource(string key, string? cultureName = null)
    {
        var culture = Culture;
        if (!string.IsNullOrWhiteSpace(cultureName) && !TryCreateCulture(cultureName, out culture))
        {
            return key;
        }

        if (TryGetCachedResource(culture.Name, key, out var resource))
        {
            return resource!;
        }

        Sync(culture);
        if (TryGetCachedResource(culture.Name, key, out resource))
        {
            return resource!;
        }

        if (TryGetCachedResource(_defaultCulture.Name, key, out resource))
        {
            return resource!;
        }

        Sync(_defaultCulture);
        if (TryGetCachedResource(_defaultCulture.Name, key, out resource))
        {
            return resource!;
        }

        return key;
    }

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

        foreach (var resourceManager in _resourceManagers)
        {
            foreach (var entry in GetResources(resourceManager, cultureInfo))
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

    private void AddResourceManagers(IEnumerable<ResourceManager> resourceManagers)
    {
        foreach (var resourceManager in resourceManagers.Where(manager => manager != null).Distinct())
        {
            if (!_resourceManagers.Contains(resourceManager))
            {
                _resourceManagers.Add(resourceManager);
            }
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Default RESX discovery uses reflection for backward compatibility; trimmed apps should pass ResourceManager or AddResourceType explicitly.")]
    private IEnumerable<ResourceManager> FindResourceManagers(IEnumerable<Assembly> assemblies)
    {
        foreach (var type in assemblies
                     .Where(assembly => assembly != null)
                     .Distinct()
                     .SelectMany(GetLoadableTypes)
                     .Where(type => type.FullName?.Contains(Mark, StringComparison.OrdinalIgnoreCase) == true))
        {
            if (TryGetResourceManagerByConvention(type, out var manager))
            {
                yield return manager;
            }
        }
    }

    private static bool TryGetResourceManager([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type resourceType, [NotNullWhen(true)] out ResourceManager? resourceManager)
    {
        return TryGetResourceManagerByConvention(resourceType, out resourceManager);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Used only by convention-based RESX discovery; trimmed apps should pass ResourceManager or AddResourceType explicitly.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Used only by convention-based RESX discovery; trimmed apps should pass ResourceManager or AddResourceType explicitly.")]
    private static bool TryGetResourceManagerByConvention(Type resourceType, [NotNullWhen(true)] out ResourceManager? resourceManager)
    {
        resourceManager = resourceType.GetProperty(nameof(ResourceManager),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?.GetValue(null, null) as ResourceManager;

        return resourceManager != null;
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
