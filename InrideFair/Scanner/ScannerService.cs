using System.Collections.Concurrent;
using InrideFair.Checkers;
using InrideFair.Database;
using InrideFair.Models;
using InrideFair.Services;
using InrideFair.Utils;

namespace InrideFair.Scanner;

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

    public async Task<ScanResult> RunScanAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        while (LogMessages.TryTake(out _))
        {
        }

        var results = new ScanResult();

        try
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                _cheatDb.ReloadSettings();

                Log($"База данных: {_cheatDb.CheatProcesses.Count} процессов, {_cheatDb.CheatFiles.Count} файлов", progress);

                Log("Проверка процессов...", progress);
                _processChecker.CheckProcesses(cancellationToken);
                results.Processes = ThreatDeduplicator.Deduplicate(_processChecker.FoundCheats);
                Log($"  Найдено процессов: {results.Processes.Count}", progress);

                cancellationToken.ThrowIfCancellationRequested();
                Log("Параллельная проверка файлов, архивов, браузеров и реестра...", progress);

                var fileTask = Task.Run(() => _fsChecker.CheckSystem(cancellationToken), cancellationToken);
                var archiveTask = Task.Run(() =>
                {
                    foreach (var suspPath in _cheatDb.SuspiciousPaths)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        _archiveChecker.CheckArchivesInDirectory(suspPath, cancellationToken);
                    }
                }, cancellationToken);
                var browserTask = Task.Run(() => _browserChecker.CheckAllBrowsers(cancellationToken), cancellationToken);
                var registryTask = Task.Run(() => _regChecker.CheckSystem(cancellationToken), cancellationToken);

                Task.WaitAll([fileTask, archiveTask, browserTask, registryTask], cancellationToken);

                results.Files = ThreatDeduplicator.Deduplicate(_fsChecker.FoundCheats);
                results.Archives = ThreatDeduplicator.Deduplicate(_archiveChecker.FoundCheats);
                results.Browser = ThreatDeduplicator.Deduplicate(_browserChecker.FoundCheats);
                results.Registry = ThreatDeduplicator.Deduplicate(_regChecker.FoundCheats);

                Log($"  Найдено файлов: {results.Files.Count}", progress);
                Log($"  Найдено архивов: {results.Archives.Count}", progress);
                Log($"  Найдено в браузерах: {results.Browser.Count}", progress);
                Log($"  Найдено в реестре: {results.Registry.Count}", progress);

                var total = results.Processes.Count + results.Files.Count + results.Archives.Count +
                            results.Browser.Count + results.Registry.Count;
                Log($"ИТОГО: {total} угроз", progress);
            }, cancellationToken);

            results.Log = [.. LogMessages];
        }
        catch (OperationCanceledException)
        {
            results.Log = [.. LogMessages];
            Log("Сканирование отменено пользователем", progress);
            _logger.Info("Сканирование отменено пользователем");
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка при сканировании", ex);
            results.Error = ex.ToString();
            Log($"ОШИБКА: {ex.Message}", progress);
            results.Log = [.. LogMessages];
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

public record ScanResult
{
    public List<DetectedThreat> Processes { get; set; } = [];
    public List<DetectedThreat> Files { get; set; } = [];
    public List<DetectedThreat> Archives { get; set; } = [];
    public List<DetectedThreat> Browser { get; set; } = [];
    public List<DetectedThreat> Registry { get; set; } = [];
    public List<string> Log { get; set; } = [];
    public string? Error { get; set; }

    public IEnumerable<DetectedThreat> AllThreats() =>
        Processes.Concat(Files).Concat(Archives).Concat(Browser).Concat(Registry);
}
