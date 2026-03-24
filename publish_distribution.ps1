# Подготовка компактной portable-версии для пользователей.

param(
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectPath = Join-Path $Root "InrideFair\InrideFair.csproj"
$ReleasePath = Join-Path $Root "Release"
$DistributionPath = Join-Path $Root "Distribution"

if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml]$projectXml = Get-Content $ProjectPath -Raw
    $Version = $projectXml.Project.PropertyGroup.Version
    if ([string]::IsNullOrWhiteSpace($Version)) {
        $Version = "1.1.0"
    }
}

$Version = $Version.Trim()

Write-Host "=== Inride Fair Distribution Builder v$Version ===" -ForegroundColor Cyan

if (Test-Path $DistributionPath) {
    Remove-Item -Path $DistributionPath -Recurse -Force
}
New-Item -ItemType Directory -Path $DistributionPath | Out-Null

& (Join-Path $Root "publish_release.ps1") -Version $Version

Write-Host "Копирование файлов в Distribution..." -ForegroundColor Yellow
Copy-Item (Join-Path $ReleasePath "InrideFair.exe") (Join-Path $DistributionPath "InrideFair.exe") -Force
Copy-Item (Join-Path $ReleasePath "config.json") (Join-Path $DistributionPath "config.json") -Force
foreach ($docFile in @("README.md", "CHANGELOG.md", "LICENSE")) {
    $source = Join-Path $Root $docFile
    if (Test-Path $source) {
        Copy-Item $source (Join-Path $DistributionPath $docFile) -Force
    }
}

$portableReadme = @"
Inride Fair v$Version
=====================

Быстрый старт:
1. Распакуйте архив в отдельную папку.
2. Запустите InrideFair.exe от имени администратора.
3. Дождитесь завершения проверки и откройте HTML-отчет.

В состав portable-версии входят:
- InrideFair.exe
- config.json
- README.md
- CHANGELOG.md
- LICENSE

Примечания:
- Для полной проверки рекомендуются права администратора.
- Программа не удаляет файлы автоматически.
- HTML и JSON отчеты создаются рядом с приложением.
"@
$portableReadme | Set-Content -Path (Join-Path $DistributionPath "README.txt") -Encoding UTF8

$zipFileName = "InrideFair_v${Version}_Portable_Windows_x64.zip"
$zipPath = Join-Path $DistributionPath $zipFileName
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Compress-Archive -Path (Join-Path $DistributionPath "InrideFair.exe"), (Join-Path $DistributionPath "config.json"), (Join-Path $DistributionPath "README.md"), (Join-Path $DistributionPath "CHANGELOG.md"), (Join-Path $DistributionPath "LICENSE"), (Join-Path $DistributionPath "README.txt") -DestinationPath $zipPath -Force

Write-Host "Готово: $DistributionPath" -ForegroundColor Green
Write-Host "Архив: $zipPath" -ForegroundColor Green
