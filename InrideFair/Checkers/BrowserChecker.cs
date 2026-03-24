using System.IO;
using System.Text.RegularExpressions;
using InrideFair.Config;
using InrideFair.Database;
using InrideFair.Models;
using InrideFair.Services;
using InrideFair.Utils;
using Microsoft.Data.Sqlite;

namespace InrideFair.Checkers;

/// <summary>
/// Проверка истории браузеров на наличие подозрительных запросов.
/// </summary>
public partial class BrowserChecker
{
    private readonly CheatDatabase _cheatDb;
    public List<DetectedThreat> FoundCheats { get; } = [];
    private readonly string _osName;
    private readonly Dictionary<string, string> _browserPaths;

    public BrowserChecker(CheatDatabase cheatDb)
    {
        _cheatDb = cheatDb;
        _osName = OperatingSystem.IsWindows() ? "Windows" : 
                  OperatingSystem.IsMacOS() ? "Darwin" : "Linux";
        _browserPaths = GetBrowserPaths();
    }

    /// <summary>
    /// Получить пути к браузерам для текущей ОС.
    /// </summary>
    private Dictionary<string, string> GetBrowserPaths()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (_osName == "Windows")
        {
            var baseDir = Path.Combine(home, "AppData", "Local");
            return new Dictionary<string, string>
            {
                ["Chrome"] = Path.Combine(baseDir, "Google", "Chrome", "User Data", "Default", "History"),
                ["Edge"] = Path.Combine(baseDir, "Microsoft", "Edge", "User Data", "Default", "History"),
                ["Yandex"] = Path.Combine(baseDir, "Yandex", "YandexBrowser", "User Data", "Default", "History"),
                ["Firefox"] = Path.Combine(home, "AppData", "Roaming", "Mozilla", "Firefox", "Profiles"),
                ["Opera"] = Path.Combine(home, "AppData", "Roaming", "Opera Software", "Opera GX Stable", "History"),
                ["Brave"] = Path.Combine(baseDir, "BraveSoftware", "Brave-Browser", "User Data", "Default", "History"),
                ["Atom"] = Path.Combine(baseDir, "Atom", "User Data", "Default", "History")
            };
        }

        if (_osName == "Darwin")
        {
            var baseDir = Path.Combine(home, "Library", "Application Support");
            return new Dictionary<string, string>
            {
                ["Chrome"] = Path.Combine(baseDir, "Google", "Chrome", "Default", "History"),
                ["Edge"] = Path.Combine(baseDir, "Microsoft Edge", "Default", "History"),
                ["Firefox"] = Path.Combine(home, "Library", "Application Support", "Firefox", "Profiles"),
                ["Opera"] = Path.Combine(baseDir, "com.operasoftware.Opera", "Default", "History"),
                ["Brave"] = Path.Combine(baseDir, "BraveSoftware", "Brave-Browser", "Default", "History"),
                ["Safari"] = Path.Combine(home, "Library", "Safari", "History.db")
            };
        }

        // Linux
        var linuxBase = Path.Combine(home, ".config");
        return new Dictionary<string, string>
        {
            ["Chrome"] = Path.Combine(linuxBase, "google-chrome", "Default", "History"),
            ["Chromium"] = Path.Combine(linuxBase, "chromium", "Default", "History"),
            ["Firefox"] = Path.Combine(home, ".mozilla", "firefox", "Profiles"),
            ["Opera"] = Path.Combine(linuxBase, "opera", "Default", "History"),
            ["Brave"] = Path.Combine(linuxBase, "BraveSoftware", "Brave-Browser", "Default", "History")
        };
    }

    /// <summary>
    /// Получить пути к кэшу браузеров.
    /// </summary>
    private Dictionary<string, List<string>> GetBrowserCachePaths()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (_osName == "Windows")
        {
            var baseDir = Path.Combine(home, "AppData", "Local");
            return new Dictionary<string, List<string>>
            {
                ["Chrome"] =
                [
                    Path.Combine(baseDir, "Google", "Chrome", "User Data", "Default", "Cache"),
                    Path.Combine(baseDir, "Google", "Chrome", "User Data", "Default", "Code Cache")
                ],
                ["Edge"] = [Path.Combine(baseDir, "Microsoft", "Edge", "User Data", "Default", "Cache")],
                ["Yandex"] = [Path.Combine(baseDir, "Yandex", "YandexBrowser", "User Data", "Default", "Cache")],
                ["Opera"] = [Path.Combine(home, "AppData", "Local", "Opera Software", "Opera GX Stable", "Cache")],
                ["Brave"] = [Path.Combine(baseDir, "BraveSoftware", "Brave-Browser", "User Data", "Default", "Cache")]
            };
        }

        if (_osName == "Darwin")
        {
            var cachesDir = Path.Combine(home, "Library", "Caches");
            return new Dictionary<string, List<string>>
            {
                ["Chrome"] = [Path.Combine(cachesDir, "Google", "Chrome", "Default", "Cache")],
                ["Firefox"] = [Path.Combine(cachesDir, "Firefox", "Profiles")],
                ["Safari"] = [Path.Combine(cachesDir, "com.apple.Safari")]
            };
        }

        // Linux
        var linuxCache = Path.Combine(home, ".cache");
        return new Dictionary<string, List<string>>
        {
            ["Chrome"] = [Path.Combine(linuxCache, "google-chrome", "Default", "Cache")],
            ["Chromium"] = [Path.Combine(linuxCache, "chromium", "Default", "Cache")],
            ["Firefox"] = [Path.Combine(home, ".cache", "mozilla", "firefox", "Profiles")]
        };
    }

    /// <summary>
    /// Получить историю браузера.
    /// </summary>
    public (List<(string url, string title, long timestamp)> urls, List<(string filepath, long timestamp)> downloads)
        GetBrowserHistory(string browserName)
    {
        return GetBrowserHistoryAsync(browserName).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Получить историю браузера (асинхронно).
    /// </summary>
    public async Task<(List<(string url, string title, long timestamp)> urls, List<(string filepath, long timestamp)> downloads)>
        GetBrowserHistoryAsync(string browserName)
    {
        var urls = new List<(string, string, long)>();
        var downloads = new List<(string, long)>();

        try
        {
            if (!_browserPaths.TryGetValue(browserName, out var historyPath))
                return (urls, downloads);

            if (!File.Exists(historyPath))
                return (urls, downloads);

            // Для Firefox (директория с профилями)
            if (browserName == "Firefox" && Directory.Exists(historyPath))
            {
                var profiles = Directory.EnumerateDirectories(historyPath);
                foreach (var profile in profiles)
                {
                    var dbPath = Path.Combine(profile, "places.sqlite");
                    if (File.Exists(dbPath))
                    {
                        var (profileUrls, profileDownloads) = await ReadFirefoxHistoryAsync(dbPath);
                        urls.AddRange(profileUrls.Take(AnalysisConstants.MaxBrowserUrls));
                        downloads.AddRange(profileDownloads.Take(AnalysisConstants.MaxBrowserDownloads));
                        break;
                    }
                }
                return (urls, downloads);
            }

            // Chrome-based браузеры
            var tempHistory = Path.GetTempFileName();
            try
            {
                FileUtils.CopyFileSafe(historyPath, tempHistory);

                var connectionString = $"Data Source={tempHistory};Mode=ReadOnly";
                using var conn = new SqliteConnection(connectionString);
                conn.Open();

                try
                {
                    // Chrome/Edge: url, title, last_visit_time
                    using var cmd = new SqliteCommand(
                        "SELECT url, title, last_visit_time FROM urls ORDER BY last_visit_time DESC LIMIT @limit", 
                        conn);
                    cmd.Parameters.AddWithValue("@limit", AnalysisConstants.MaxBrowserUrls);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        urls.Add((
                            reader.GetString(0),
                            reader.IsDBNull(1) ? "" : reader.GetString(1),
                            reader.IsDBNull(2) ? 0 : reader.GetInt64(2)
                        ));
                    }
                }
                catch (SqliteException)
                {
                    // Таблица urls не найдена
                }

                try
                {
                    // Downloads: target_path, start_time
                    using var cmd = new SqliteCommand(
                        "SELECT target_path, start_time FROM downloads ORDER BY start_time DESC LIMIT @limit",
                        conn);
                    cmd.Parameters.AddWithValue("@limit", AnalysisConstants.MaxBrowserDownloads);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        downloads.Add((
                            reader.GetString(0),
                            reader.IsDBNull(1) ? 0 : reader.GetInt64(1)
                        ));
                    }
                }
                catch (SqliteException)
                {
                    // Таблица downloads не найдена
                }
            }
            finally
            {
                try { File.Delete(tempHistory); } catch { }
            }
        }
        catch (Exception)
        {
            // Игнорируем ошибки
        }

        return (urls, downloads);
    }

    private async Task<(List<(string, string, long)>, List<(string, long)>)> ReadFirefoxHistoryAsync(string dbPath)
    {
        var urls = new List<(string, string, long)>();
        var downloads = new List<(string, long)>();

        try
        {
            var tempDb = Path.GetTempFileName();
            try
            {
                FileUtils.CopyFileSafe(dbPath, tempDb);

                var connectionString = $"Data Source={tempDb};Mode=ReadOnly";
                using var conn = new SqliteConnection(connectionString);
                await conn.OpenAsync();

                try
                {
                    // Firefox: url, title, last_visit_date
                    using var cmd = new SqliteCommand(
                        "SELECT url, title, last_visit_date FROM moz_places ORDER BY last_visit_date DESC LIMIT @limit",
                        conn);
                    cmd.Parameters.AddWithValue("@limit", AnalysisConstants.MaxBrowserUrls);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        urls.Add((
                            reader.GetString(0),
                            reader.IsDBNull(1) ? "" : reader.GetString(1),
                            reader.IsDBNull(2) ? 0 : reader.GetInt64(2)
                        ));
                    }
                }
                catch (SqliteException) { }

                try
                {
                    // Downloads: target_path, start_time
                    using var cmd = new SqliteCommand(
                        "SELECT target_path, start_time FROM downloads ORDER BY start_time DESC LIMIT @limit",
                        conn);
                    cmd.Parameters.AddWithValue("@limit", AnalysisConstants.MaxBrowserDownloads);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        downloads.Add((
                            reader.GetString(0),
                            reader.IsDBNull(1) ? 0 : reader.GetInt64(1)
                        ));
                    }
                }
                catch (SqliteException)
                {
                    // Таблица downloads не найдена
                }
            }
            finally
            {
                try { File.Delete(tempDb); } catch { }
            }
        }
        catch (Exception ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning($"Ошибка при чтении истории Firefox: {dbPath}", ex);
        }

        return (urls, downloads);
    }

    private (List<(string, string, long)>, List<(string, long)>) ReadFirefoxHistory(string dbPath)
    {
        var urls = new List<(string, string, long)>();
        var downloads = new List<(string, long)>();

        try
        {
            var tempDb = Path.GetTempFileName();
            try
            {
                FileUtils.CopyFileSafe(dbPath, tempDb);

                var connectionString = $"Data Source={tempDb};Mode=ReadOnly";
                using var conn = new SqliteConnection(connectionString);
                conn.Open();

                try
                {
                    // Firefox: url, title, last_visit_date
                    using var cmd = new SqliteCommand(
                        "SELECT url, title, last_visit_date FROM moz_places ORDER BY last_visit_date DESC LIMIT @limit",
                        conn);
                    cmd.Parameters.AddWithValue("@limit", AnalysisConstants.MaxBrowserUrls);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        urls.Add((
                            reader.GetString(0),
                            reader.IsDBNull(1) ? "" : reader.GetString(1),
                            reader.IsDBNull(2) ? 0 : reader.GetInt64(2)
                        ));
                    }
                }
                catch (SqliteException) { }
            }
            finally
            {
                try { File.Delete(tempDb); } catch { }
            }
        }
        catch (Exception ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning($"Ошибка при чтении истории браузера. {ex.Message}");
        }

        return (urls, downloads);
    }

    /// <summary>
    /// Конвертировать Chrome timestamp в дату.
    /// </summary>
    private static string ConvertChromeTimestamp(long timestamp)
    {
        if (timestamp == 0)
            return "N/A";

        try
        {
            var epochStart = new DateTime(1601, 1, 1);
            var deltaSeconds = timestamp / 1_000_000;
            var resultDate = epochStart.AddSeconds(deltaSeconds);
            return resultDate.ToString("dd.MM.yyyy HH:mm");
        }
        catch
        {
            return "N/A";
        }
    }

    /// <summary>
    /// Конвертировать Firefox timestamp в дату.
    /// </summary>
    private static string ConvertFirefoxTimestamp(long timestamp)
    {
        if (timestamp == 0)
            return "N/A";

        try
        {
            var epochStart = new DateTime(1970, 1, 1);
            var deltaSeconds = timestamp / 1_000_000;
            var resultDate = epochStart.AddSeconds(deltaSeconds);
            return resultDate.ToString("dd.MM.yyyy HH:mm");
        }
        catch
        {
            return "N/A";
        }
    }

    /// <summary>
    /// Форматировать дату для браузера.
    /// </summary>
    private string FormatBrowserDate(long timestamp, string browserName)
    {
        if (timestamp == 0)
            return "N/A";

        return browserName == "Firefox" 
            ? ConvertFirefoxTimestamp(timestamp) 
            : ConvertChromeTimestamp(timestamp);
    }

    /// <summary>
    /// Проверить URL на подозрительность.
    /// </summary>
    public string? CheckUrl(string url, string title = "")
    {
        var combined = $"{url} {title}".ToLower();
        foreach (var query in CheatSignatures.SuspiciousQueries)
        {
            if (combined.Contains(query.ToLower()))
                return query;
        }
        return null;
    }

    /// <summary>
    /// Проверить путь к файлу на подозрительность.
    /// </summary>
    public string? CheckFilePath(string filepath)
    {
        var pathLower = filepath.ToLower();
        foreach (var query in CheatSignatures.SuspiciousQueries)
        {
            if (pathLower.Contains(query.ToLower()))
                return query;
        }

        try
        {
            var filename = Path.GetFileName(filepath).ToLower();
            foreach (var cheatFile in _cheatDb.CheatFiles)
            {
                if (filename.Contains(cheatFile.ToLower()))
                    return cheatFile;
            }
        }
        catch { }

        return null;
    }
}
