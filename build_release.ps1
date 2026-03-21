# Скрипт для автоматической сборки релизной версии Inride Fair (C# .NET 11)
# Создаёт EXE-файл и готовит пакет для распространения

param(
    [string]$Version = ""
)

Write-Host "=" -ForegroundColor Cyan
Write-Host "  Inride Fair Release Builder (C#)" -ForegroundColor Cyan
Write-Host "=" -ForegroundColor Cyan

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectPath = Join-Path $Root "InrideFair\InrideFair.csproj"
$ReleaseDir = Join-Path $Root "release"

# Получение версии из проекта
if ([string]::IsNullOrEmpty($Version)) {
    $ProjectXml = [Xml.XmlDocument](Get-Content $ProjectPath -Raw)
    $Version = $ProjectXml.Project.PropertyGroup.Version
    if ([string]::IsNullOrEmpty($Version)) {
        $Version = "1.0.0"
    }
}

# Проверка .NET
Write-Host "`n  Проверка .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($null -eq $dotnetVersion) {
    Write-Host "  ERROR: .NET SDK не найден!" -ForegroundColor Red
    exit 1
}
Write-Host "  .NET SDK: v$dotnetVersion" -ForegroundColor Green

# Очистка
Write-Host "`n  Очистка..." -ForegroundColor Yellow
if (Test-Path "$Root\bin") { Remove-Item -Recurse -Force "$Root\bin" }
if (Test-Path "$Root\obj") { Remove-Item -Recurse -Force "$Root\obj" }
if (Test-Path $ReleaseDir) { Remove-Item -Recurse -Force $ReleaseDir }
Write-Host "  Очистка завершена" -ForegroundColor Green

# Сборка
Write-Host "`n  Сборка Release..." -ForegroundColor Yellow
dotnet publish "$ProjectPath" `
    -c Release `
    -r win-x64 `
    --self-contained `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o "$ReleaseDir\publish"

if ($LASTEXITCODE -ne 0) {
    Write-Host "  ERROR: Ошибка сборки!" -ForegroundColor Red
    exit 1
}

# Подготовка релиза
Write-Host "`n  Подготовка релиза..." -ForegroundColor Yellow

# Копирование config.json
if (Test-Path "$Root\config.json") {
    Copy-Item "$Root\config.json" "$ReleaseDir\publish\config.json"
    Write-Host "  Скопирован: config.json" -ForegroundColor Green
}

# Копирование документации
foreach ($docFile in @("README.md", "LICENSE")) {
    if (Test-Path "$Root\$docFile") {
        Copy-Item "$Root\$docFile" "$ReleaseDir\publish\$docFile"
        Write-Host "  Скопирован: $docFile" -ForegroundColor Green
    }
}

# Создание файла версии
$VersionContent = @"
Inride Fair v$Version
Дата сборки: $(Get-Date -Format "dd.MM.yyyy HH:mm:ss")
.NET: $dotnetVersion
ОС: Windows x64

© 2026 Inride Software. Все права защищены.
"@
$VersionContent | Out-File -FilePath "$ReleaseDir\publish\VERSION.txt" -Encoding UTF8
Write-Host "  Создан: VERSION.txt" -ForegroundColor Green

# Создание INSTALL.txt
$InstallContent = @"
================================================================================
  Inride Fair v$Version - Инструкция по установке и запуску
================================================================================

ТРЕБОВАНИЯ:
  - Windows 10/11 (64-bit)
  - .NET: НЕ ТРЕБУЕТСЯ (встроен в EXE)
  - Права администратора (для полной проверки системы)

ЗАПУСК:
  1. Скопируйте все файлы из этой папки в удобное место
  2. Запустите InrideFair.exe от имени администратора
  3. Нажмите "Начать проверку"

ФАЙЛЫ:
  - InrideFair.exe    - Основной исполняемый файл (~62 MB)
  - config.json       - Файл конфигурации
  - README.md         - Полная документация
  - VERSION.txt       - Информация о версии

ПРИМЕЧАНИЯ:
  - При первом запуске может потребоваться подтверждение от Windows Defender
  - Некоторые антивирусы могут ложно срабатывать на упакованные приложения
  - Для корректной работы не удаляйте config.json

ПОДДЕРЖКА:
  © 2026 Inride Software
  Email: freno@inride.software
================================================================================
"@
$InstallContent | Out-File -FilePath "$ReleaseDir\publish\INSTALL.txt" -Encoding UTF8
Write-Host "  Создан: INSTALL.txt" -ForegroundColor Green

# Вывод размера
$ExePath = "$ReleaseDir\publish\InrideFair.exe"
if (Test-Path $ExePath) {
    $ExeSize = (Get-Item $ExePath).Length / 1MB
    Write-Host "`n  Размер EXE: $([math]::Round($ExeSize, 2)) MB" -ForegroundColor Cyan
}

# Создание архива
Write-Host "`n  Создание архива..." -ForegroundColor Yellow
$ArchiveName = "InrideFair_v$($Version.Replace('.', '_'))_Windows"
$ArchivePath = "$ReleaseDir\$ArchiveName.zip"

if (Test-Path "C:\Program Files\7-Zip\7z.exe") {
    & "C:\Program Files\7-Zip\7z.exe" a -tzip "$ArchivePath" "$ReleaseDir\publish\*"
    Write-Host "  Создан архив: $ArchiveName.zip" -ForegroundColor Green
} else {
    # Используем встроенный Compress-Archive
    Compress-Archive -Path "$ReleaseDir\publish\*" -DestinationPath "$ArchivePath" -Force
    Write-Host "  Создан архив: $ArchiveName.zip" -ForegroundColor Green
}

# Итог
Write-Host "`n" "=" -ForegroundColor Cyan
Write-Host "  Готово!" -ForegroundColor Green
Write-Host "=" -ForegroundColor Cyan
Write-Host "`n  Релиз находится в: $ReleaseDir" -ForegroundColor Cyan
Write-Host "  Архив: $ArchivePath" -ForegroundColor Cyan
Write-Host "`n  Сборка завершена успешно!" -ForegroundColor Green
