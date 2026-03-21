# Скрипт публикации Inride Fair - чистая версия для распространения
# Создаёт папку с только необходимыми файлами для пользователей

$ErrorActionPreference = "Stop"

$PROJECT_PATH = "InrideFair\InrideFair.csproj"
$RELEASE_PATH = "Release"
$DISTRIBUTION_PATH = "Distribution"
$CONFIGURATION = "Release"

Write-Host "=== Публикация Inride Fair (Чистая версия) ===" -ForegroundColor Cyan
Write-Host "Конфигурация: $CONFIGURATION" -ForegroundColor Gray

# Очистка папки Distribution
Write-Host "`n[1/4] Очистка папки Distribution..." -ForegroundColor Yellow
if (Test-Path $DISTRIBUTION_PATH) {
    Remove-Item -Path $DISTRIBUTION_PATH\* -Recurse -Force -ErrorAction SilentlyContinue
} else {
    New-Item -ItemType Directory -Path $DISTRIBUTION_PATH | Out-Null
}

# Публикация проекта
Write-Host "`n[2/4] Публикация проекта..." -ForegroundColor Yellow
dotnet publish $PROJECT_PATH `
    -c $CONFIGURATION `
    --self-contained true `
    -r win-x64 `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    --output $RELEASE_PATH

if ($LASTEXITCODE -ne 0) {
    Write-Host "Ошибка публикации!" -ForegroundColor Red
    exit 1
}

# Копирование необходимых файлов в Distribution
Write-Host "`n[3/4] Копирование файлов для распространения..." -ForegroundColor Yellow

# Копируем только EXE и конфигурацию
Copy-Item "$RELEASE_PATH\InrideFair.exe" -Destination "$DISTRIBUTION_PATH\" -Force
Copy-Item "InrideFair\config.json" -Destination "$DISTRIBUTION_PATH\" -Force
Copy-Item "LICENSE" -Destination "$DISTRIBUTION_PATH\" -Force
Copy-Item "README.md" -Destination "$DISTRIBUTION_PATH\" -Force

# Создаём краткую инструкцию
$readmeContent = @"
# Inride Fair v1.0.0 - Система обнаружения читов

## 🚀 Быстрый старт

1. Запустите `InrideFair.exe` от имени администратора (рекомендуется)
2. Нажмите кнопку "НАЧАТЬ ПРОВЕРКУ"
3. Дождитесь завершения сканирования
4. Просмотрите отчёт (откроется автоматически)

## 📋 Системные требования

- ОС: Windows 10/11 x64
- Память: 512 MB RAM
- Место на диске: 100 MB

## 🎮 Возможности

✅ Сканирование процессов
✅ Проверка файлов
✅ Анализ архивов
✅ Проверка браузеров
✅ Сканирование реестра
✅ HTML и JSON отчёты

## ⚠️ Важно

- Некоторые антивирусы могут ложно срабатывать на сканер
- Для полной проверки требуются права администратора
- Программа не удаляет файлы автоматически

## 📞 Поддержка

Если возникли вопросы или проблемы:
1. Проверьте лог файл в папке Logs
2. Убедитесь что у вас есть права администратора
3. Попробуйте запустить от имени администратора

## 📄 Лицензия

© 2026 Inride Software. Все права защищены.

---
**Версия:** 1.0.0  
**Дата сборки:** $(Get-Date -Format "dd.MM.yyyy")  
**Платформа:** Windows x64
"@

$readmeContent | Out-File -FilePath "$DISTRIBUTION_PATH\README.txt" -Encoding UTF8

Write-Host "  ✓ InrideFair.exe" -ForegroundColor Green
Write-Host "  ✓ config.json" -ForegroundColor Green
Write-Host "  ✓ LICENSE" -ForegroundColor Green
Write-Host "  ✓ README.md" -ForegroundColor Green
Write-Host "  ✓ README.txt (краткая инструкция)" -ForegroundColor Green

# Итог
Write-Host "`n=== Публикация завершена ===" -ForegroundColor Green
Write-Host "Папка для распространения: $((Get-Item $DISTRIBUTION_PATH).FullName)" -ForegroundColor Gray

$exePath = Join-Path (Get-Location) "$DISTRIBUTION_PATH\InrideFair.exe"
$exeSize = (Get-ChildItem $exePath).Length / 1MB
Write-Host "Размер EXE: $([math]::Round($exeSize, 2)) MB" -ForegroundColor Gray

$totalSize = (Get-ChildItem $DISTRIBUTION_PATH -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Общий размер: $([math]::Round($totalSize, 2)) MB" -ForegroundColor Gray

# Создание ZIP архива
Write-Host "`n[4/4] Создание ZIP архива..." -ForegroundColor Yellow

$zipFileName = "InrideFair_v1.0.0_$(Get-Date -Format 'yyyyMMdd_HHmmss').zip"
$zipPath = Join-Path (Get-Location) $zipFileName

# Удаляем старый архив если существует
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

# Создаём ZIP архив
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($DISTRIBUTION_PATH, $zipPath)

$zipSize = (Get-Item $zipPath).Length / 1MB
Write-Host "  ✓ $zipFileName ($([math]::Round($zipSize, 2)) MB)" -ForegroundColor Green

Write-Host "`nГотово к распространению! 📦" -ForegroundColor Cyan
Write-Host "Архив создан: $zipPath" -ForegroundColor Cyan
