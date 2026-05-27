# 更新日志

## 12.0.3.3 - 2026-05-27

- 🔨[优化]-保持 Avalonia 12 为唯一支持的 Avalonia 版本线，并将 Demo 扩展为可在 `net8.0`、`net9.0`、`net10.0`、`net11.0` 构建。
- 📝[文档]-说明实际目标框架边界：Avalonia 12 包提供 `net8.0`/`net10.0` 资产，因此不降级到 Avalonia 11 时无法支持 `net6.0`/`net7.0`。

## 12.0.3.2 - 2026-05-27

- 🔨[修复]-合并 PR #5，并修复 AOT 友好的 `I18nExtension` 路径，确保运行时返回底层 `MultiBinding` 值。
- 🔨[优化]-保留当前 Avalonia 12 项目/包配置，并将 PR 改动适配到 sealed `MultiBinding` API。

## 12.0.3.1 - 2026-05-20

- 🔨[优化]-将 Avalonia 与 Avalonia.Desktop 升级到 `12.0.3`。
- 🔨[优化]-将 `System.Text.Json` 升级到最新稳定版 `10.0.8`。
- 🔨[优化]-`Prism.Avalonia` 与 `Prism.DryIoc.Avalonia` 继续保留在开源 `8.1.97.11073` 版本线，不升级到商业化的 9.x 版本线。
- 📝[文档]-同步更新第三方开源组件审计说明中的包版本信息。

## 12.0.2.1 - 2026-05-08

- 🔨[优化]-将按包拆分的 README 文件合并到根目录 `README.md`，降低维护成本。
- 🔨[优化]-新增 `README.zh-CN.md` 作为简体中文 README，遵循英文默认、中文使用 `zh-CN` 后缀的命名规范。
- 🔨[优化]-将技术说明文档合并并迁移到 `docs/design.md` 和 `docs/design.zh-CN.md`。
- 🔨[优化]-将文档图示整理到 `docs/assets`，简体中文图示放到 `docs/assets/zh-CN`。
- 🔨[优化]-更新 NuGet 包 README 打包配置，所有包统一包含中英文 README。
- 🔨[优化]-移除已废弃的按包 README 文件和旧技术文档文件。
- 🔨[优化]-移除示例项目中针对 Lang.Avalonia 包裁剪的旧 `Roots.xml` 引用。
- 😄[新增]-新增支持显式 `ResourceManager` 或资源 Designer 类型的 RESX 注册 API，适配裁剪发布。
- 🐛[修复]-RESX 示例改为显式传入 `ResourceManager` 注册 `ResxLangPlugin`，避免裁剪发布时为 Lang.Avalonia.Resx 配置 Root.xml。
- 🐛[修复]-Analysis 示例 `ViewLocator` 改为显式类型映射，避免裁剪发布下的字符串反射问题。
- 🐛[修复]-将示例项目的传递依赖 `System.Drawing.Common` 固定到 `10.0.6`，避免安全告警。
- 📝[文档]-补充说明 JSON/XML 提供器和显式注册的 RESX 资源不要求使用方为 Lang.Avalonia NuGet 包配置 Root.xml。
