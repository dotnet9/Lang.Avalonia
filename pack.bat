@echo off
setlocal

pushd "%~dp0"

set "CONFIGURATION=Release"
set "ARTIFACTS_DIR=%CD%\artifacts"
set "PACKAGES_DIR=%ARTIFACTS_DIR%\packages"

if exist "%PACKAGES_DIR%" rmdir /s /q "%PACKAGES_DIR%"
mkdir "%PACKAGES_DIR%"

echo [1/3] Restoring solution...
dotnet restore Lang.Avalonia.slnx
if errorlevel 1 goto :error

echo [2/3] Building solution...
dotnet build Lang.Avalonia.slnx -c %CONFIGURATION% --no-restore
if errorlevel 1 goto :error

echo [3/3] Packing libraries...
for %%P in (
    "src\Lang.Avalonia\Lang.Avalonia.csproj"
    "src\Lang.Avalonia.Analysis\Lang.Avalonia.Analysis.csproj"
    "src\Lang.Avalonia.Json\Lang.Avalonia.Json.csproj"
    "src\Lang.Avalonia.Resx\Lang.Avalonia.Resx.csproj"
    "src\Lang.Avalonia.Xml\Lang.Avalonia.Xml.csproj"
) do (
    dotnet pack %%~P -c %CONFIGURATION% --no-build -o "%PACKAGES_DIR%"
    if errorlevel 1 goto :error
)

echo.
echo Packages are available in:
echo %PACKAGES_DIR%

popd
exit /b 0

:error
set "EXIT_CODE=%ERRORLEVEL%"
echo.
echo Pack failed with exit code %EXIT_CODE%.
popd
exit /b %EXIT_CODE%
