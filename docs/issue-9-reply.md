# Issue #9 回复草稿（web 似乎无法使用）

> 以下内容可直接粘贴到 GitHub Issue 回复。

感谢反馈，问题已定位并在浏览器中复现与验证。

**结论：Lang.Avalonia 本身支持 Avalonia Browser（WebAssembly），但 `JsonLangPlugin` 默认的"扫描输出目录"加载方式在浏览器上不可用。**

`JsonLangPlugin` 默认从 `AppDomain.CurrentDomain.BaseDirectory` 用 `Directory.GetFiles` 扫描并 `File.ReadAllText` 读取语言文件。桌面程序中语言 JSON 拷贝到输出目录后可以正常读到；但浏览器端发布产物是 wwwroot 下的静态文件，由 HTTP 服务提供，WASM 运行时的 `System.IO` 看不到它们，于是语言列表为空（`LanguagesLoaded: 0`）、界面显示原始 Key。整个过程不会抛异常（诊断只记录在 `LoadDiagnostics` 中），所以没有任何报错。

## 解决办法：改用嵌入资源加载

1. csproj 中把语言 JSON 从拷贝到输出目录改为（或同时）嵌入：

```xml
<ItemGroup>
  <EmbeddedResource Include="I18n\*.json" />
</ItemGroup>
```

2. 注册前把主程序集加入插件：

```csharp
var plugin = new JsonLangPlugin();
plugin.AddResource(typeof(App).Assembly);
I18nManager.Instance.Register(plugin, new CultureInfo("zh-CN"));
```

该方式在桌面与浏览器上行为一致。

## 另外请检查 Browser 工程的三个配置

我们做了一个可复现的浏览器示例（`src/Lang.Avalonia.Json.BrowserDemo`），排查过程中发现浏览器工程还有几个容易踩的坑，供参考：

1. **工程 SDK**：请使用 `Microsoft.NET.Sdk.WebAssembly` + `net10.0-browser`（或 net8/net9-browser），并安装 `dotnet workload install wasm-tools`；需要 .NET 10 SDK。
2. **字体**：FluentTheme 默认字体 Inter 在浏览器上没有系统回退，必须嵌入字体（如 `.WithInterFont()`；若要正常显示中文，还需嵌入中文字体并配置 `FontManagerOptions.FontFallbacks`），否则首次渲染会抛 `Could not create glyphTypeface`，表现为白屏卡在加载页。您工程里的 `.WithFont()` 扩展请确认在浏览器上确实加载了字体。
3. **生命周期**：Avalonia 12 的浏览器生命周期是 `IActivityApplicationLifetime`，主视图请通过 `MainViewFactory` 提供（参考 Avalonia 官方模板）。

详细的配置清单见仓库文档：[docs/browser-support.md](../docs/browser-support.md)，示例工程 `src/Lang.Avalonia.Json.BrowserDemo` 的诊断面板可以直接对比"文件夹扫描"与"嵌入资源"两种模式在浏览器上的表现。

后续版本计划在语言资源为空时向控制台输出警告，避免静默失败。
