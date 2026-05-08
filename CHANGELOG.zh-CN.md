# 更新日志

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
