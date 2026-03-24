using System.IO;
using System.Text.RegularExpressions;
using InrideFair.Config;
using InrideFair.Database;
using InrideFair.Models;
using InrideFair.Services;

namespace InrideFair.Checkers;

/// <summary>
/// BrowserChecker - продолжение (проверка кэша, логов, сессий).
/// </summary>
public partial class BrowserChecker
{
    /// <summary>
    /// Проверить браузер.
    /// </summary>
    public (int foundQueries, int foundDownloads) CheckBrowser(string browserName)
    {
        var (urls, downloads) = GetBrowserHistory(browserName);
        var foundQueries = 0;
        var foundDownloads = 0;

        foreach (var (url, title, timestamp) in urls.Take(AnalysisConstants.MaxBrowserUrlsCheck))
        {
            var visitDate = FormatBrowserDate(timestamp, browserName);
            var match = CheckUrl(url, title);
            
            if (match != null)
            {
                FoundCheats.Add(new DetectedThreat
                {
                    Type = "browser_search",
                    Browser = browserName,
                    Url = url,
                    Title = title,
                    Match = match,
                    Risk = "medium"
                });
                foundQueries++;
            }
        }

        foreach (var (filepath, timestamp) in downloads.Take(AnalysisConstants.MaxBrowserDownloadsCheck))
        {
            var downloadDate = FormatBrowserDate(timestamp, browserName);
            var match = CheckFilePath(filepath);
            
            if (match != null)
            {
                FoundCheats.Add(new DetectedThreat
                {
                    Type = "browser_download",
                    Browser = browserName,
                    FilePath = filepath,
                    Path = filepath,
                    Match = match,
                    Risk = "high"
                });
                foundDownloads++;
            }
        }

        return (foundQueries, foundDownloads);
    }

    /// <summary>
    /// Сканировать кэш браузера.
    /// </summary>
    public List<DetectedThreat> ScanCacheDirectory(List<string> cachePaths, string browserName)
    {
        var found = new List<DetectedThreat>();

        foreach (var cacheDir in cachePaths)
        {
            if (!Directory.Exists(cacheDir))
                continue;

            try
            {
                var cacheFiles = new List<FileInfo>();
                var dirInfo = new DirectoryInfo(cacheDir);
                
                foreach (var item in dirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
                {
                    if (!item.Name.StartsWith('.'))
                        cacheFiles.Add(item);
                    
                    if (cacheFiles.Count >= 100)
                        break;
                }

                foreach (var cacheFile in cacheFiles)
                {
                    var parsedData = ParseCacheFile(cacheFile.FullName);
                    foreach (var (url, snippet) in parsedData)
                    {
                        var match = CheckUrl(url, snippet);
                        if (match != null)
                        {
                            found.Add(new DetectedThreat
                            {
                                Type = "browser_cache",
                                Browser = browserName,
                                Url = url,
                                FilePath = cacheFile.FullName,
                                Path = cacheFile.FullName,
                                Match = match,
                                Risk = "medium"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = ServiceContainer.GetService<ILoggingService>();
                logger?.Warning($"Ошибка при проверке кэша браузера: {browserName}", ex);
            }
        }

        return found;
    }

    /// <summary>
    /// Распарсить файл кэша браузера.
    /// </summary>
    private List<(string url, string snippet)> ParseCacheFile(string cachePath)
    {
        var results = new List<(string, string)>();

        try
        {
            var file = new FileInfo(cachePath);
            if (!file.Exists || file.Extension.ToLower() is not ("" or ".dat" or ".bin" or ".cache"))
                return results;

            using var fs = File.OpenRead(cachePath);
            var buffer = new byte[1024 * 1024]; // 1MB
            var bytesRead = fs.Read(buffer, 0, buffer.Length);
            var content = buffer.Take(bytesRead).ToArray();

            // Поиск URL в бинарных данных
            var urlPattern = @"https?://[a-zA-Z0-9\-._~:/?#\[\]@!$&'()*+,;=%]+";
            var matches = Regex.Matches(System.Text.Encoding.UTF8.GetString(content), urlPattern);

            foreach (Match match in matches.Take(50))
            {
                var url = match.Value;
                var contentStart = System.Text.Encoding.UTF8.GetString(content).IndexOf(url);
                var snippet = contentStart > 0
                    ? System.Text.Encoding.UTF8.GetString(content, contentStart, Math.Min(200, bytesRead - contentStart))
                    : "";
                results.Add((url, snippet));
            }
        }
        catch (Exception ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning($"Ошибка при парсинге кэш файла: {cachePath}", ex);
        }

        return results;
    }

    /// <summary>
    /// Сканировать лог файл браузера.
    /// </summary>
    public List<DetectedThreat> ScanLogFile(string logPath, string browserName)
    {
        var found = new List<DetectedThreat>();

        try
        {
            if (!File.Exists(logPath))
                return found;

            // Для директорий (Crash Reports) сканируем содержимое
            if ((new FileInfo(logPath).Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var dirInfo = new DirectoryInfo(logPath);
                foreach (var logFile in dirInfo.EnumerateFiles("*.log"))
                {
                    found.AddRange(ScanLogFile(logFile.FullName, browserName));
                }
                return found;
            }

            var lineNumber = 0;
            foreach (var line in File.ReadLines(logPath))
            {
                lineNumber++;
                var urlPattern = @"https?://[a-zA-Z0-9\-._~:/?#\[\]@!$&'()*+,;=%]+";
                var urls = Regex.Matches(line, urlPattern);

                foreach (Match match in urls.Take(10))
                {
                    var url = match.Value;
                    var urlMatch = CheckUrl(url, line);
                    if (urlMatch != null)
                    {
                        found.Add(new DetectedThreat
                        {
                            Type = "browser_log",
                            Browser = browserName,
                            Url = url,
                            FilePath = logPath,
                            Path = logPath,
                            Match = urlMatch,
                            Risk = "medium"
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning($"Ошибка при сканировании логов браузера: {browserName}", ex);
        }

        return found;
    }

    /// <summary>
    /// Сканировать файлы сессии браузера.
    /// </summary>
    public List<DetectedThreat> ScanSessionFiles(string sessionDir)
    {
        var found = new List<DetectedThreat>();

        if (!Directory.Exists(sessionDir))
            return found;

        var sessionPatterns = new[] { "Sessions", "Session Storage", "Local Storage", "Last Session", "Last Tabs" };

        try
        {
            var dirInfo = new DirectoryInfo(sessionDir);
            foreach (var item in dirInfo.EnumerateFiles())
            {
                var isSessionFile = sessionPatterns.Any(p => item.Name.Contains(p, StringComparison.OrdinalIgnoreCase));
                if (!isSessionFile)
                    continue;

                try
                {
                    using var fs = File.OpenRead(item.FullName);
                    var buffer = new byte[2 * 1024 * 1024]; // 2MB max
                    var bytesRead = fs.Read(buffer, 0, buffer.Length);
                    var content = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var urlPattern = @"https?://[a-zA-Z0-9\-._~:/?#\[\]@!$&'()*+,;=%]+";
                    var urls = Regex.Matches(content, urlPattern);

                    foreach (Match match in urls.Take(20))
                    {
                        var url = match.Value;
                        foreach (var query in CheatSignatures.SuspiciousQueries)
                        {
                            if (url.ToLower().Contains(query.ToLower()))
                            {
                                found.Add(new DetectedThreat
                                {
                                    Type = "browser_session",
                                    Browser = "Session Recovery",
                                    Url = url,
                                    FilePath = item.FullName,
                                    Path = item.FullName,
                                    Match = query,
                                    Risk = "medium"
                                });
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = ServiceContainer.GetService<ILoggingService>();
                    logger?.Warning($"Ошибка при чтении файла сессии: {item.FullName}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning($"Ошибка при сканировании файлов сессии: {sessionDir}", ex);
        }

        return found;
    }

    /// <summary>
    /// Сканировать DNS кэш.
    /// </summary>
    public List<DetectedThreat> ScanDnsCache()
    {
        var found = new List<DetectedThreat>();

        if (_osName == "Windows")
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/displaydns",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    StandardOutputEncoding = System.Text.Encoding.GetEncoding(866)
                };

                using var process = System.Diagnostics.Process.Start(startInfo);
                var output = process?.StandardOutput.ReadToEnd() ?? "";
                process?.WaitForExit();

                var dnsPattern = @"Record Name .+?: (.+?)\n";
                var matches = Regex.Matches(output, dnsPattern, RegexOptions.Multiline);

                foreach (Match match in matches.Take(500))
                {
                    var entry = match.Groups[1].Value.Trim();
                    var urlMatch = CheckUrl(entry, "");
                    if (urlMatch != null)
                    {
                        found.Add(new DetectedThreat
                        {
                            Type = "dns_cache",
                            Browser = "System DNS",
                            Url = entry,
                            Match = urlMatch,
                            Risk = "low"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = ServiceContainer.GetService<ILoggingService>();
                logger?.Warning("Ошибка при проверке DNS кэша", ex);
            }
        }
        else if (_osName is "Darwin" or "Linux")
        {
            // Проверка /etc/hosts
            try
            {
                foreach (var line in File.ReadLines("/etc/hosts"))
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith('#'))
                    {
                        var urlMatch = CheckUrl(line, "");
                        if (urlMatch != null)
                        {
                            found.Add(new DetectedThreat
                            {
                                Type = "hosts_file",
                                Browser = "System Hosts",
                                Url = line.Trim(),
                                Match = urlMatch,
                                Risk = "high"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = ServiceContainer.GetService<ILoggingService>();
                logger?.Warning("Ошибка при чтении /etc/hosts", ex);
            }
        }

        return found;
    }

    /// <summary>
    /// Сканировать Windows Prefetch.
    /// </summary>
    public List<DetectedThreat> ScanPrefetch()
    {
        var found = new List<DetectedThreat>();

        if (_osName != "Windows")
            return found;

        var prefetchDir = new DirectoryInfo(@"C:\Windows\Prefetch");
        if (!prefetchDir.Exists)
            return found;

        try
        {
            foreach (var prefetchFile in prefetchDir.EnumerateFiles("*.pf"))
            {
                try
                {
                    using var fs = File.OpenRead(prefetchFile.FullName);
                    var buffer = new byte[fs.Length];
                    fs.ReadExactly(buffer, 0, buffer.Length);

                    var exePattern = @"[A-Za-z0-9_\-]+\.exe";
                    var matches = Regex.Matches(System.Text.Encoding.UTF8.GetString(buffer), exePattern);

                    foreach (Match match in matches.Take(10))
                    {
                        var exeName = match.Value.ToLower();
                        foreach (var cheatSig in _cheatDb.CheatFiles)
                        {
                            if (exeName.Contains(cheatSig.ToLower()))
                            {
                                found.Add(new DetectedThreat
                                {
                                    Type = "prefetch",
                                    Browser = "Windows Prefetch",
                                    Name = exeName,
                                    FilePath = prefetchFile.FullName,
                                    Path = prefetchFile.FullName,
                                    Match = cheatSig,
                                    Risk = "high"
                                });
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = ServiceContainer.GetService<ILoggingService>();
                    logger?.Warning($"Ошибка при чтении prefetch файла: {prefetchFile.FullName}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning("Ошибка при сканировании Windows Prefetch", ex);
        }

        return found;
    }

    /// <summary>
    /// Проверить все браузеры.
    /// </summary>
    public int CheckAllBrowsers()
    {
        var totalQueries = 0;
        var totalDownloads = 0;
        var totalCache = 0;
        var totalLogs = 0;
        var totalSessions = 0;
        var totalDns = 0;
        var totalPrefetch = 0;

        // Проверка истории
        foreach (var browserName in _browserPaths.Keys)
        {
            var (queries, downloads) = CheckBrowser(browserName);
            totalQueries += queries;
            totalDownloads += downloads;
        }

        // Проверка кэша
        var cachePaths = GetBrowserCachePaths();
        foreach (var (browserName, paths) in cachePaths)
        {
            var cacheFindings = ScanCacheDirectory(paths, browserName);
            FoundCheats.AddRange(cacheFindings);
            totalCache += cacheFindings.Count;
        }

        // Проверка логов (упрощённо)
        // Можно добавить пути к логам при необходимости

        // Проверка сессионных файлов
        foreach (var browserName in _browserPaths.Keys)
        {
            var sessionDir = Path.GetDirectoryName(_browserPaths[browserName]);
            if (!string.IsNullOrEmpty(sessionDir) && Directory.Exists(sessionDir))
            {
                var sessionFindings = ScanSessionFiles(sessionDir);
                FoundCheats.AddRange(sessionFindings);
                totalSessions += sessionFindings.Count;
            }
        }

        // Проверка DNS кэша
        var dnsFindings = ScanDnsCache();
        FoundCheats.AddRange(dnsFindings);
        totalDns = dnsFindings.Count;

        // Проверка Prefetch (Windows)
        if (_osName == "Windows")
        {
            var prefetchFindings = ScanPrefetch();
            FoundCheats.AddRange(prefetchFindings);
            totalPrefetch = prefetchFindings.Count;
        }

        var total = totalQueries + totalDownloads + totalCache + totalLogs + totalSessions + totalDns + totalPrefetch;
        return total;
    }
}
