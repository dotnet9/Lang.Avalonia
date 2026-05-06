@echo off
setlocal enabledelayedexpansion

set "project_paths=src\Lang.Avalonia.Analysis.Demo src\Lang.Avalonia.Json.Demo src\Lang.Avalonia.Resx.Demo src\Lang.Avalonia.Xml.Demo"
set "platforms=win-x64 linux-x64"

call "%~dp0publishbase.bat" "%project_paths%" "%platforms%"
