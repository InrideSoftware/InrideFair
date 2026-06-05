using System.IO;
using InrideFair.Config;
using InrideFair.Database;
using InrideFair.Models;
using InrideFair.Services;
using InrideFair.Utils;

namespace InrideFair.Checkers;

/// <summary>
/// Проверка файловой системы на наличие читов.
/// </summary>
public class FileSystemChecker
{
    private readonly CheatDatabase _cheatDb;
    private readonly ILoggingService _logger;
    public List<DetectedThreat> FoundCheats { get; } = [];

    public FileSystemChecker(CheatDatabase cheatDb, ILoggingService logger)
    {
        _cheatDb = cheatDb;
        _logger = logger;
    }

    /// <summary>
    /// Эвристический анализ директории.
    /// </summary>
    public (int score, List<string> indicators) AnalyzeDirectoryHeuristic(string dirPath)
    {
        var suspicionScore = 0;
        var indicators = new List<string>();

        try
        {
            var path = new DirectoryInfo(dirPath);
            if (!path.Exists)
                return (0, indicators);

            var files = new List<string>();
            var subdirs = new List<string>();

            try
            {
                foreach (var item in path.EnumerateFileSystemInfos())
                {
                    if (item is FileInfo)
                        files.Add(item.Name.ToLowerInvariant());
                    else if (item is DirectoryInfo)
                        subdirs.Add(item.Name.ToLowerInvariant());
                }
            }
            catch (UnauthorizedAccessException)
            {
                return (0, indicators);
            }

            foreach (var f in files)
            {
                if (CheatSignatures.LoaderKeywords.Any(kw => f.Contains(kw, StringComparison.Ordinal)))
                {
                    suspicionScore += 2;
                    indicators.Add($"Loader: {f}");
                }
            }

            foreach (var f in files)
            {
                if (f.EndsWith(".dll", StringComparison.Ordinal) &&
                    CheatSignatures.DllKeywords.Any(kw => f.Contains(kw, StringComparison.Ordinal)))
                {
                    suspicionScore += 1;
                    indicators.Add($"DLL: {f}");
                }
            }

            foreach (var f in files)
            {
                if (CheatSignatures.ConfigFiles.Contains(f))
                {
                    suspicionScore += 1;
                    indicators.Add($"Config: {f}");
                }
            }

            foreach (var d in subdirs)
            {
                if (CheatSignatures.SuspiciousFolderNames.Any(cf => d.Contains(cf, StringComparison.Ordinal)))
                {
                    suspicionScore += 2;
                    indicators.Add($"Folder: {d}");
                }
            }

            var luaFiles = files.Count(f => f.EndsWith(".lua", StringComparison.Ordinal));
            if (luaFiles > 0)
            {
                suspicionScore += 1;
                indicators.Add($"Lua: {luaFiles}");
            }

            var dllCount = files.Count(f => f.EndsWith(".dll", StringComparison.Ordinal));
            if (dllCount >= 3)
            {
                suspicionScore += 1;
                indicators.Add($"DLL count: {dllCount}");
            }

            var dirLower = dirPath.ToLowerInvariant();
            if (CheatSignatures.GameKeywords.Any(gk => dirLower.Contains(gk, StringComparison.Ordinal)) &&
                !CheatSignatures.LegitimateGameDirKeywords.Any(lk => dirLower.Contains(lk, StringComparison.Ordinal)))
            {
                suspicionScore += 1;
                indicators.Add($"Game dir: {dirPath}");
            }

            suspicionScore = Math.Min(suspicionScore, AnalysisConstants.MaxHeuristicScore);
        }
        catch (Exception)
        {
            return (0, indicators);
        }

        return (suspicionScore, indicators);
    }

    /// <summary>
    /// Получить метку подозрительности.
    /// </summary>
    public string GetSuspicionLabel(int score)
    {
        return score switch
        {
            0 => "Чисто",
            1 => "Низкая",
            2 => "Средняя",
            _ => "ВЫСОКАЯ"
        };
    }

    /// <summary>
    /// Проверить директорию.
    /// </summary>
    public List<DetectedThreat> CheckDirectory(string directory, CancellationToken cancellationToken = default)
    {
        var found = new List<DetectedThreat>();

        try
        {
            var path = new DirectoryInfo(directory);
            if (!path.Exists)
                return found;

            var dirPathLower = path.FullName.ToLowerInvariant();
            if (_cheatDb.IsExcluded(dirPathLower))
                return found;

            var isGameDir = _cheatDb.IsLegitimateGamePath(dirPathLower);
            var filesToCheck = EnumerateFilesLimited(path, AnalysisConstants.MaxScanDepth, AnalysisConstants.MaxFilesPerDirectory, cancellationToken);

            foreach (var item in filesToCheck)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var nameLower = item.Name.ToLowerInvariant();
                    var itemPathLower = item.FullName.ToLowerInvariant();

                    if (_cheatDb.IsSystemFile(itemPathLower))
                        continue;

                    if (ReportFileDetector.IsReportFilePath(item.FullName))
                        continue;

                    if (isGameDir && nameLower.EndsWith(".dll", StringComparison.Ordinal))
                    {
                        var isKnownCheat = new[] { "osiris", "neverlose", "primordial", "paste" }
                            .Any(cheat => nameLower.Contains(cheat, StringComparison.Ordinal));
                        if (!isKnownCheat)
                            continue;
                    }

                    var foundMatch = false;
                    foreach (var cheatFile in _cheatDb.CheatFiles)
                    {
                        if (!SignatureMatcher.MatchesFileName(nameLower, cheatFile))
                            continue;

                        found.Add(new DetectedThreat
                        {
                            Type = "file",
                            Path = item.FullName,
                            Match = cheatFile,
                            Hash = FileAnalyzer.GetFileHash(item.FullName) ?? "",
                            ExactMatch = true,
                            Risk = "high"
                        });
                        foundMatch = true;
                        break;
                    }

                    if (!foundMatch)
                    {
                        foreach (var cheatSig in _cheatDb.CheatProcesses)
                        {
                            if (!SignatureMatcher.MatchesFileName(nameLower, cheatSig))
                                continue;

                            found.Add(new DetectedThreat
                            {
                                Type = "file",
                                Path = item.FullName,
                                Match = cheatSig,
                                ExactMatch = false,
                                Risk = "medium"
                            });
                            break;
                        }
                    }

                    var extension = item.Extension.ToLowerInvariant();
                    if ((extension is ".dll" or ".exe") && item.Length < 2 * 1024 * 1024)
                    {
                        var (score, indicators) = FileAnalyzer.AnalyzeDllFile(item.FullName);
                        if (score >= 5)
                        {
                            found.Add(new DetectedThreat
                            {
                                Type = "file",
                                Path = item.FullName,
                                Match = $"Analysis: {score}/10",
                                Hash = FileAnalyzer.GetFileHash(item.FullName) ?? "",
                                AnalysisScore = score,
                                Indicators = indicators,
                                Risk = score >= 8 ? "high" : "medium"
                            });
                        }
                    }

                    if (extension is ".json" or ".ini" or ".cfg")
                    {
                        var (score, indicators) = FileAnalyzer.AnalyzeConfigFile(item.FullName);
                        if (score >= 5)
                        {
                            found.Add(new DetectedThreat
                            {
                                Type = "file",
                                Path = item.FullName,
                                Match = $"Config: {score}/10",
                                AnalysisScore = score,
                                Indicators = indicators,
                                Risk = score >= 8 ? "high" : "medium"
                            });
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.Debug($"Нет доступа к файлу при сканировании: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.Debug($"Ошибка при обработке файла: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Warning($"Ошибка при сканировании директории: {ex.Message}");
        }

        return found;
    }

    /// <summary>
    /// Проверить систему.
    /// </summary>
    public int CheckSystem(CancellationToken cancellationToken = default)
    {
        var allFound = new List<DetectedThreat>();

        foreach (var suspPath in _cheatDb.SuspiciousPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            allFound.AddRange(CheckDirectoryWithHeuristic(suspPath, cancellationToken));
        }

        if (OperatingSystem.IsWindows())
        {
            var programData = Environment.GetEnvironmentVariable("PROGRAMDATA") ?? @"C:\ProgramData";
            cancellationToken.ThrowIfCancellationRequested();
            allFound.AddRange(CheckDirectoryWithHeuristic(programData, cancellationToken));
        }

        FoundCheats.AddRange(allFound);
        return allFound.Count;
    }

    private List<DetectedThreat> CheckDirectoryWithHeuristic(string directory, CancellationToken cancellationToken)
    {
        var found = CheckDirectory(directory, cancellationToken);
        var (score, indicators) = AnalyzeDirectoryHeuristic(directory);

        if (score >= AnalysisConstants.HeuristicThreatScoreThreshold)
        {
            found.Add(new DetectedThreat
            {
                Type = "directory_heuristic",
                Path = directory,
                Match = $"Heuristic: {GetSuspicionLabel(score)} ({score})",
                AnalysisScore = score,
                Indicators = indicators,
                Risk = score >= 3 ? "high" : "medium"
            });
        }

        return found;
    }

    internal static IEnumerable<FileInfo> EnumerateFilesLimited(
        DirectoryInfo root,
        int maxDepth,
        int maxFiles,
        CancellationToken cancellationToken)
    {
        var count = 0;
        var queue = new Queue<(DirectoryInfo Directory, int Depth)>();
        queue.Enqueue((root, 0));

        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (directory, depth) = queue.Dequeue();
            if (depth > maxDepth)
                continue;

            IEnumerable<FileSystemInfo> entries;
            try
            {
                entries = directory.EnumerateFileSystemInfos();
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (entry is FileInfo file)
                {
                    yield return file;
                    count++;
                    if (count >= maxFiles)
                        yield break;
                }
                else if (entry is DirectoryInfo subDirectory && depth < maxDepth)
                {
                    queue.Enqueue((subDirectory, depth + 1));
                }
            }

            if (count >= maxFiles)
                yield break;
        }
    }
}
