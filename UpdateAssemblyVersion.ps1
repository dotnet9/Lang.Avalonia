[CmdletBinding()]
param(
    [Alias('AssemblyInfoFile')]
    [string]$ProjectFile = (Join-Path $PSScriptRoot 'Directory.Build.props'),

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+\.\d+$')]
    [string]$Version
)

$resolvedProjectFile = (Resolve-Path -LiteralPath $ProjectFile -ErrorAction Stop).Path
$content = [System.IO.File]::ReadAllText($resolvedProjectFile)
$versionPattern = '(?m)^(?<indent>[ \t]*)<Version>(?<value>[^<]+)</Version>[ \t]*$'
$matches = [regex]::Matches($content, $versionPattern)

if ($matches.Count -ne 1) {
    throw "Expected exactly one four-part <Version> element in '$resolvedProjectFile', found $($matches.Count)."
}

$currentVersion = $matches[0].Groups['value'].Value.Trim()
if ($currentVersion -eq $Version) {
    Write-Output "Version is already $Version in $resolvedProjectFile."
    exit 0
}

$replacement = '$1<Version>' + $Version + '</Version>'
$updatedContent = [regex]::Replace($content, $versionPattern, $replacement, 1)
[System.IO.File]::WriteAllText($resolvedProjectFile, $updatedContent, [System.Text.UTF8Encoding]::new($false))

Write-Output "Updated version from $currentVersion to $Version in $resolvedProjectFile."
