using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Media;

[assembly: SupportedOSPlatform("browser")]

namespace Lang.Avalonia.Json.BrowserDemo;

internal sealed partial class Program
{
    /// <summary>
    /// 嵌入的思源黑体资源，为 Inter 缺失的 CJK 字形提供回退。
    /// </summary>
    private const string CjkFontUri = "avares://Lang.Avalonia.Json.BrowserDemo/Assets#Source Han Sans CN";

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .WithInterFont()
            .With(new FontManagerOptions
            {
                FontFallbacks = [new FontFallback { FontFamily = new FontFamily(CjkFontUri) }],
            });

    public static Task Main(string[] args) => BuildAvaloniaApp()
        .StartBrowserAppAsync("out");
}
