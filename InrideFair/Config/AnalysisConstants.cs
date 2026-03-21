namespace InrideFair.Config;

/// <summary>
/// Константы анализа файлов.
/// </summary>
public static class AnalysisConstants
{
    public const int MaxDllAnalysisSize = 5 * 1024 * 1024; // 5 MB
    public const int MaxDllReadSize = 2 * 1024 * 1024; // 2 MB
    public const int MaxConfigAnalysisScore = 10;
    public const int MaxDllAnalysisScore = 10;
    public const int MaxHeuristicScore = 3;

    // Подозрительные размеры файлов (в KB)
    public static readonly int[] SuspiciousFileSizes = [
        20051, 15110, 13030, 13485, 17487, 4311, 16809, 15090
    ];

    public const int MinSuspiciousSizeKb = 10 * 1024; // 10 MB
    public const int MaxSuspiciousSizeKb = 20 * 1024; // 20 MB

    // Лимиты истории браузеров
    public const int MaxBrowserUrls = 1000;
    public const int MaxBrowserDownloads = 500;
    public const int MaxBrowserUrlsCheck = 500;
    public const int MaxBrowserDownloadsCheck = 200;

    // Константы сканирования директорий
    public const int MaxScanDepth = 2;
    public const int ProgressThreshold = 100;
    public const int ProgressBarLength = 30;
}
