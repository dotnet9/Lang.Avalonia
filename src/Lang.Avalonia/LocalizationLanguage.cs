using System.Collections.Generic;

namespace Lang.Avalonia;

/// <summary>
/// 描述一种已加载的本地化语言及其资源字典。
/// </summary>
public class LocalizationLanguage
{
    /// <summary>
    /// 面向界面展示的语言名称。
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 对该语言的补充说明。
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 标准文化名称，例如 zh-CN、en-US。
    /// </summary>
    public string CultureName { get; set; } = null!;

    /// <summary>
    /// 语言资源表，Key 为完整资源路径，Value 为翻译文本。
    /// </summary>
    public Dictionary<string, string> Languages { get; } = new();
}
