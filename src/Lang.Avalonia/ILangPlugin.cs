using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Lang.Avalonia;

/// <summary>
/// 多语言资源插件契约。不同资源格式只需要实现该接口，核心库即可统一完成查找与切换。
/// </summary>
public interface ILangPlugin
{
    /// <summary>
    /// 加载插件资源，并设置默认文化。
    /// </summary>
    void Load(CultureInfo cultureInfo);

    /// <summary>
    /// 从额外程序集追加语言资源，适用于模块化应用或插件式应用。
    /// </summary>
    void AddResource(params Assembly[] assemblies);

    /// <summary>
    /// 当前正在使用的文化。
    /// </summary>
    public CultureInfo Culture { get; set; }

    /// <summary>
    /// 获取插件当前已知的语言列表。
    /// </summary>
    List<LocalizationLanguage>? GetLanguages();

    /// <summary>
    /// 获取指定 Key 的语言文本。未命中时由插件决定回退策略。
    /// </summary>
    string GetResource(string key, string? cultureName = null);
}
