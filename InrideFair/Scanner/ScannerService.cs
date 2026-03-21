using System.Collections.Concurrent;
using InrideFair.Checkers;
using InrideFair.Database;
using InrideFair.Services;

namespace InrideFair.Scanner;

/// <summary>
/// Сервис сканирования системы.
/// </summary>
public class ScannerService : IDisposable
{
    private readonly ILoggingService _logger;
    private readonly CheatDatabase _cheatDb;
    private readonly ProcessChecker _processChecker;
    private readonly FileSystemChecker _fsChecker;
    private readonly ArchiveChecker _archiveChecker;
    private readonly BrowserChecker _browserChecker;
    private readonly RegistryChecker _regChecker;
    private bool _disposed;

    public ConcurrentBag<string> LogMessages { get; } = new();

    public ScannerService(
        ILoggingService logger,
        CheatDatabase cheatDb,
        ProcessChecker processChecker,
        FileSystemChecker fsChecker,
        ArchiveChecker archiveChecker,
        BrowserChecker browserChecker,
        RegistryChecker regChecker)
    {
        _logger = logger;
        _cheatDb = cheatDb;
        _processChecker = processChecker;
        _fsChecker = fsChecker;
        _archiveChecker = archiveChecker;
        _browserChecker = browserChecker;
        _regChecker = regChecker;
    }

    /// <summary>
    /// Выполнить сканирование и вернуть результаты.
    /// </summary>
    public async Task<ScanResult> RunScanAsync(IProgress<string>? progress = null)
    {
        var results = new ScanResult
        {
            Processes = [],
            Files = [],
            Archives = [],
            Browser = [],
            Registry = [],
            Log = [.. LogMessages],
            Error = null
        };

        try
        {
            await Task.Run(async () =>
            {
                // Инициализация
                Log($"База данных: {_cheatDb.CheatProcesses.Count} процессов, {_cheatDb.CheatFiles.Count} файлов", progress);

                // 1. Процессы
                Log("Проверка процессов...", progress);
                await Task.Run(() => _processChecker.CheckProcesses());
                results.Processes = [.. _processChecker.FoundCheats];
                Log($"  Найдено процессов: {results.Processes.Count}", progress);

                // 2. Файлы
                Log("Проверка файлов...", progress);
                await Task.Run(() => _fsChecker.CheckSystem());
                results.Files = [.. _fsChecker.FoundCheats];
                Log($"  Найдено файлов: {results.Files.Count}", progress);

                // 3. Архивы
                Log("Проверка архивов...", progress);
                await Task.Run(() =>
                {
                    foreach (var suspPath in _cheatDb.SuspiciousPaths)
                    {
                        _archiveChecker.CheckArchivesInDirectory(suspPath);
                    }
                });
                results.Archives = [.. _archiveChecker.FoundCheats];
                Log($"  Найдено архивов: {results.Archives.Count}", progress);

                // 4. Браузеры
                Log("Проверка браузеров...", progress);
                await Task.Run(() => _browserChecker.CheckAllBrowsers());
                results.Browser = [.. _browserChecker.FoundCheats];
                Log($"  Найдено в браузерах: {results.Browser.Count}", progress);

                // 5. Реестр
                Log("Проверка реестра...", progress);
                await Task.Run(() => _regChecker.CheckSystem());
                results.Registry = [.. _regChecker.FoundCheats];
                Log($"  Найдено в реестре: {results.Registry.Count}", progress);

                // Итог
                var total = results.Processes.Count + results.Files.Count + results.Archives.Count +
                           results.Browser.Count + results.Registry.Count;
                Log($"ИТОГО: {total} угроз", progress);
            });
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка при сканировании", ex);
            results.Error = ex.ToString();
            Log($"ОШИБКА: {ex.Message}", progress);
        }

        return results;
    }

    private void Log(string message, IProgress<string>? progress = null)
    {
        LogMessages.Add(message);
        _logger.Debug(message);
        progress?.Report(message);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.Info("ScannerService disposed");
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Результаты сканирования.
/// </summary>
public record ScanResult
{
    public List<Dictionary<string, object?>> Processes { get; set; } = [];
    public List<Dictionary<string, object?>> Files { get; set; } = [];
    public List<Dictionary<string, object?>> Archives { get; set; } = [];
    public List<Dictionary<string, object?>> Browser { get; set; } = [];
    public List<Dictionary<string, object?>> Registry { get; set; } = [];
    public List<string> Log { get; set; } = [];
    public string? Error { get; set; }
}
