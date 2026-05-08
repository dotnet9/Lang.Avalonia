# Changelog

## 12.0.2.1 - 2026-05-08

- 🔨[Optimized]-Merged package-specific README files into the root `README.md` for easier maintenance.
- 🔨[Optimized]-Added `README.zh-CN.md` as the Simplified Chinese README, following the English-default and `zh-CN` suffix naming convention.
- 🔨[Optimized]-Moved and merged technical design documentation into `docs/design.md` and `docs/design.zh-CN.md`.
- 🔨[Optimized]-Moved documentation diagrams to `docs/assets`, with Simplified Chinese diagrams under `docs/assets/zh-CN`.
- 🔨[Optimized]-Updated NuGet package README packing so all packages include the shared English and Chinese README files.
- 🔨[Optimized]-Removed obsolete package-specific README files and old technical document files.
- 🔨[Optimized]-Removed demo-level `Roots.xml` references that were previously used for Lang.Avalonia package trimming.
- 😄[Added]-Added trim-friendly RESX registration APIs that accept an explicit `ResourceManager` or generated resource designer type.
- 🐛[Fixed]-Updated the RESX demo to register `ResxLangPlugin` with an explicit `ResourceManager`, avoiding Root.xml requirements for Lang.Avalonia.Resx under trimming.
- 🐛[Fixed]-Updated the Analysis demo `ViewLocator` to avoid string-based reflection under trimming.
- 🐛[Fixed]-Pinned transitive `System.Drawing.Common` to `10.0.6` for demo dependency security hygiene.
- 📝[Docs]-Documented that JSON/XML providers and explicitly registered RESX resources do not require consumer Root.xml configuration for Lang.Avalonia NuGet packages.
