# Скрипт публикации Inride Fair
# Создаёт полностью готовую версию в папке Release

$ErrorActionPreference = "Stop"

$PROJECT_PATH = "InrideFair\InrideFair.csproj"
$RELEASE_PATH = "Release"
$CONFIGURATION = "Release"

Write-Host "=== Публикация Inride Fair ===" -ForegroundColor Cyan
Write-Host "Конфигурация: $CONFIGURATION" -ForegroundColor Gray

# Очистка папки Release
Write-Host "`n[1/3] Очистка папки Release..." -ForegroundColor Yellow
if (Test-Path $RELEASE_PATH) {
    Remove-Item -Path $RELEASE_PATH\* -Recurse -Force -ErrorAction SilentlyContinue
} else {
    New-Item -ItemType Directory -Path $RELEASE_PATH | Out-Null
}

# Публикация
Write-Host "`n[2/3] Публикация проекта..." -ForegroundColor Yellow
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

# Копирование config.json
Write-Host "`n[3/3] Копирование конфигурации..." -ForegroundColor Yellow
Copy-Item "InrideFair\config.json" -Destination "$RELEASE_PATH\" -Force

# Итог
Write-Host "`n=== Публикация завершена ===" -ForegroundColor Green
Write-Host "Папка публикации: $((Get-Item $RELEASE_PATH).FullName)" -ForegroundColor Gray

$exePath = Join-Path (Get-Location) "$RELEASE_PATH\InrideFair.exe"
Write-Host "EXE файл: $exePath" -ForegroundColor Gray

$size = (Get-ChildItem $exePath).Length / 1MB
Write-Host "Размер: $([math]::Round($size, 2)) MB" -ForegroundColor Gray
