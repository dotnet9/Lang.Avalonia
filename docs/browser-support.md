# 浏览器（WebAssembly）支持指南

本文基于 `Lang.Avalonia.Json.BrowserDemo` 在 Avalonia 12.1.3 + .NET 10（`net10.0-browser`）下的实测结论，说明各插件在 Avalonia Browser 上的可用性、工程配置要求与常见问题。

## 结论速览

- 核心库（`I18nManager`、`{c:I18n}` 标记扩展、绑定刷新、运行时切换文化）在浏览器上**完全可用**，已实测验证。
- JSON / XML 插件的**嵌入资源模式（`AddResource`）可用**；**输出目录扫描模式不可用**——浏览器 WASM 环境中 `System.IO` 无法访问随站点部署的静态文件（wwwroot 由 HTTP 服务提供，不在虚拟文件系统内），导致语言列表为空、界面显示原始 Key，且整个过程不抛异常（诊断只记在 `LoadDiagnostics` 中）。
- 若语言文件已改为嵌入资源，桌面与浏览器可共用同一份注册代码。

## 插件可用性矩阵

| 插件 | 嵌入资源（`AddResource`） | 输出目录扫描（默认） |
| --- | --- | --- |
| `JsonLangPlugin` | ✅ 已实测 | ❌ 浏览器不可用 |
| `XmlLangPlugin` | 同机制，可用（待实测） | ❌ 浏览器不可用 |
| `ResxLangPlugin` | ✅ 走 ResourceManager / 卫星程序集 | — |

## 示例：Lang.Avalonia.Json.BrowserDemo

该示例演示两种加载模式，由 `App.UseEmbeddedResources` 开关控制：

- `true`（默认）：注册前调用 `plugin.AddResource(typeof(App).Assembly)`，语言 JSON 以 `EmbeddedResource` 嵌入主程序集，浏览器可用。
- `false`：`JsonLangPlugin` 默认的输出目录扫描模式，用于复现 dotnet9/Lang.Avalonia#9，浏览器端 `LanguagesLoaded: 0`、界面显示 Key。

主界面内置诊断面板，展示 `ResourceFolder`、`DirectoryExists`、`LanguagesLoaded`、当前文化与 `LoadDiagnostics`，便于对比两种模式。

### 构建与运行

```bash
# 需要 .NET 10 SDK + wasm-tools 工作负载（demo 内 global.json 已钉定 SDK 版本）
dotnet workload install wasm-tools
dotnet run --project src/Lang.Avalonia.Json.BrowserDemo
# 或发布后用任意静态服务器托管 wwwroot（需支持 br/gz 预压缩协商）
dotnet publish src/Lang.Avalonia.Json.BrowserDemo -c Debug
```

## 浏览器工程配置清单（四个必踩的坑）

### 1. 工程 SDK 与目标框架

使用官方浏览器工程姿势，不要用旧的 plain SDK + RuntimeIdentifier 写法：

```xml
<Project Sdk="Microsoft.NET.Sdk.WebAssembly">
  <PropertyGroup>
    <TargetFramework>net10.0-browser</TargetFramework>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia.Browser" />
  </ItemGroup>
</Project>
```

- 需要先安装 `dotnet workload install wasm-tools`。
- 选用 `net10.0-browser`（Avalonia 官方模板与 Semi.Avalonia Demo.Web 同款）。实测 `net11.0-browser` 在 .NET 11 RC 存在 emcc 链接错误（`undefined symbol: saveSetjmp`），plain SDK + `RuntimeIdentifier=browser-wasm` 则解析不到运行时包。
- 引用库的多目标列表包含 `net11.0` 时，用 SDK 10 构建浏览器工程需要裁剪该目标（见 demo 被引用库 csproj 中的条件 `TargetFrameworks`）；SDK 11+ 构建不受影响。

### 2. 字体（最常见的白屏原因）

FluentTheme 默认字体为 Inter，浏览器端**没有系统字体回退**。不嵌入字体时，首次文本排版会抛出：

```
Could not create glyphTypeface. Font family: Inter (key: compositefont:fonts:Inter#Inter)
```

该异常发生在渲染阶段，表现为**应用初始化正常完成但首帧永不渲染、页面停留在 splash**，控制台才有上述错误。

必须为浏览器工程配置字体：

- 拉丁字符：`.WithInterFont()`（Avalonia.Fonts.Inter 包）；
- 中文等 CJK 字形：嵌入思源黑体等中文字体，并通过 `FontManagerOptions.FontFallbacks` 回退（Inter 不含 CJK 字形，否则中文显示为方块），参考 `BrowserDemo/Program.cs` 与 `Assets/` 目录，亦可参考 Semi.Avalonia.Demo.Fonts 的做法。

### 3. 应用生命周期（Avalonia 12）

Avalonia 12 浏览器端生命周期为 `IActivityApplicationLifetime`，主视图通过工厂提供；旧的 `ISingleViewApplicationLifetime` 在浏览器上单独使用不会显示视图：

```csharp
if (ApplicationLifetime is IActivityApplicationLifetime activityLifetime)
{
    activityLifetime.MainViewFactory = () => new MainView();
}
else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
{
    singleViewPlatform.MainView = new MainView();
}
```

### 4. Splash 隐藏

首帧渲染后 Avalonia 仅给 `.avalonia-splash` 元素添加 `splash-close` CSS 类，**隐藏动作由页面 CSS 完成**。`index.html` 必须包含（官方模板 app.css 已内置）：

```css
.avalonia-splash.splash-close {
    transition: opacity 200ms, display 200ms;
    display: none;
    opacity: 0;
}
```

缺少该规则时 splash 会一直覆盖在应用上方，容易被误判为"启动卡死"。

## 诊断建议

- 浏览器 F12 控制台是第一现场：启动异常大多被运行时吞掉，只在此处可见。
- `JsonLangPlugin.LoadDiagnostics` 可读取加载诊断；`Resources.Count == 0` 时仅记录 "Please provide valid language JSON files."，不会抛异常。
- index.html / main.js 建议保持官方模板结构（`OverrideHtmlAssetPlaceholders` + `StaticWebAssetFingerprintPattern` + importmap + `main.js` 引导脚本）。

## 参考来源

- Avalonia 官方跨平台模板（avalonia-dotnet-templates）中的 Browser 工程。
- Semi.Avalonia `demo/Semi.Avalonia.Demo.Web` 及其字体工程 `Semi.Avalonia.Demo.Fonts`（思源黑体，SIL OFL 协议）。
- [Avalonia WASM 排错文档](https://docs.avaloniaui.net/troubleshooting/platform-specific-issues/webassembly)。
