param(
    [string]$Version = ""
)

$scriptPath = Join-Path $PSScriptRoot "build_release.ps1"
& $scriptPath -Version $Version
