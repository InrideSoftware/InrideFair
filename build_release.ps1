# Сборка и упаковка полной релизной версии Inride Fair.

param(
  [string]$Version = ""
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectPath = Join-Path $Root "InrideFair\InrideFair.csproj"
$ProjectDir = Join-Path $Root "InrideFair"
$ReleaseDir = Join-Path $Root "Release"

if ([string]::IsNullOrWhiteSpace($Version)) {
  [xml]$projectXml = Get-Content $ProjectPath -Raw
  $Version = $projectXml.Project.PropertyGroup.Version
  if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = "1.1.0"
  }
}

$Version = $Version.Trim()

Write-Host "=== Inride Fair Release Builder v$Version ===" -ForegroundColor Cyan

$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion) {
  throw ".NET SDK не найден. Установите .NET 11 SDK или новее."
}

Write-Host "[1/4] Очистка старых артефактов..." -ForegroundColor Yellow
foreach ($path in @(
  (Join-Path $ProjectDir "bin"),
  (Join-Path $ProjectDir "obj"),
  $ReleaseDir
)) {
  if (Test-Path $path) {
    Remove-Item -Path $path -Recurse -Force
  }
}
New-Item -ItemType Directory -Path $ReleaseDir | Out-Null

Write-Host "[2/4] Публикация self-contained сборки..." -ForegroundColor Yellow
dotnet publish $ProjectPath `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DebugType=None `
  -p:DebugSymbols=false `
  -o $ReleaseDir

if ($LASTEXITCODE -ne 0) {
  throw "Ошибка публикации проекта."
}

Write-Host "[3/4] Копирование документации..." -ForegroundColor Yellow
Copy-Item (Join-Path $ProjectDir "config.json") (Join-Path $ReleaseDir "config.json") -Force
foreach ($docFile in @("README.md", "CHANGELOG.md", "CONTRIBUTING.md", "LICENSE")) {
  $source = Join-Path $Root $docFile
  if (Test-Path $source) {
    Copy-Item $source (Join-Path $ReleaseDir $docFile) -Force
  }
}

$versionInfo = @"
Inride Fair v$Version
Дата сборки: $(Get-Date -Format "dd.MM.yyyy HH:mm:ss")
.NET SDK: $dotnetVersion
Платформа: Windows x64
Формат: self-contained single-file

© 2026 Inride Software. Все права защищены.
"@
$versionInfo | Set-Content -Path (Join-Path $ReleaseDir "VERSION.txt") -Encoding UTF8

$installInfo = @"
Inride Fair v$Version
=====================

1. Запустите InrideFair.exe от имени администратора.
2. Убедитесь, что рядом лежит config.json.
3. После завершения проверки используйте HTML/JSON отчет.

В комплекте:
- InrideFair.exe
- config.json
- README.md
- CHANGELOG.md
- CONTRIBUTING.md
- LICENSE
- VERSION.txt
"@
$installInfo | Set-Content -Path (Join-Path $ReleaseDir "INSTALL.txt") -Encoding UTF8

Write-Host "[4/4] Создание ZIP-архива..." -ForegroundColor Yellow
$archiveName = "InrideFair_v${Version}_Windows_x64.zip"
$archivePath = Join-Path $ReleaseDir $archiveName
if (Test-Path $archivePath) {
  Remove-Item $archivePath -Force
}
Compress-Archive -Path (Join-Path $ReleaseDir "*") -DestinationPath $archivePath -Force

Write-Host "Готово: $ReleaseDir" -ForegroundColor Green
Write-Host "Архив: $archivePath" -ForegroundColor Green
