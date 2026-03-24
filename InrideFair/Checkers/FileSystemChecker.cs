using System.IO;
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
                        files.Add(item.Name.ToLower());
                    else if (item is DirectoryInfo)
                        subdirs.Add(item.Name.ToLower());
                }
            }
            catch (UnauthorizedAccessException)
            {
                return (0, indicators);
            }

            // Проверка на loader/injector файлы
            foreach (var f in files)
            {
                if (Config.CheatSignatures.LoaderKeywords.Any(kw => f.Contains(kw)))
                {
                    suspicionScore += 2;
                    indicators.Add($"Loader: {f}");
                }
            }

            // Проверка на подозрительные DLL
            foreach (var f in files)
            {
                if (f.EndsWith(".dll") && Config.CheatSignatures.DllKeywords.Any(kw => f.Contains(kw)))
                {
                    suspicionScore += 1;
                    indicators.Add($"DLL: {f}");
                }
            }

            // Проверка конфигов читов
            foreach (var f in files)
            {
                if (Config.CheatSignatures.ConfigFiles.Contains(f))
                {
                    suspicionScore += 1;
                    indicators.Add($"Config: {f}");
                }
            }

            // Проверка папок с названиями читов
            foreach (var d in subdirs)
            {
                if (Config.CheatSignatures.SuspiciousFolderNames.Any(cf => d.Contains(cf)))
                {
                    suspicionScore += 2;
                    indicators.Add($"Folder: {d}");
                }
            }

            // Проверка Lua скриптов
            var luaFiles = files.Where(f => f.EndsWith(".lua")).ToList();
            if (luaFiles.Count > 0)
            {
                suspicionScore += 1;
                indicators.Add($"Lua: {luaFiles.Count}");
            }

            // Проверка количества DLL
            var dllCount = files.Count(f => f.EndsWith(".dll"));
            if (dllCount >= 3)
            {
                suspicionScore += 1;
                indicators.Add($"DLL count: {dllCount}");
            }

            // Проверка на игровую директорию (не Steam)
            var dirLower = dirPath.ToLower();
            if (Config.CheatSignatures.GameKeywords.Any(gk => dirLower.Contains(gk)) &&
                !Config.CheatSignatures.LegitimateGameDirKeywords.Any(lk => dirLower.Contains(lk)))
            {
                suspicionScore += 1;
                indicators.Add($"Game dir: {dirPath}");
            }

            suspicionScore = Math.Min(suspicionScore, Config.AnalysisConstants.MaxHeuristicScore);
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
    public List<DetectedThreat> CheckDirectory(string directory)
    {
        var found = new List<DetectedThreat>();

        try
        {
            var path = new DirectoryInfo(directory);
            if (!path.Exists)
                return found;

            var dirPathLower = path.FullName.ToLower();
            if (_cheatDb.IsExcluded(dirPathLower))
                return found;

            var isGameDir = _cheatDb.IsLegitimateGamePath(dirPathLower);

            // Сбор файлов для анализа
            var filesToCheck = new List<FileInfo>();
            try
            {
                foreach (var item in path.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        filesToCheck.Add(item);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // Пропускаем недоступные файлы
                        _logger.Debug($"Нет доступа к файлу: {ex.Message}");
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Warning($"Нет доступа к директории: {ex.Message}");
                return found;
            }

            foreach (var item in filesToCheck)
            {
                try
                {
                    var nameLower = item.Name.ToLower();
                    var itemPathLower = item.FullName.ToLower();

                    // Пропуск системных файлов
                    if (_cheatDb.IsSystemFile(itemPathLower))
                        continue;

                    // Пропуск DLL в игровых директориях (кроме известных читов)
                    if (isGameDir && nameLower.EndsWith(".dll"))
                    {
                        var isKnownCheat = new[] { "osiris", "neverlose", "primordial", "paste" }
                            .Any(cheat => nameLower.Contains(cheat));
                        if (!isKnownCheat)
                            continue;
                    }

                    // Проверка по имени файла
                    var foundMatch = false;
                    foreach (var cheatFile in _cheatDb.CheatFiles)
                    {
                        if (nameLower == cheatFile.ToLower())
                        {
                            found.Add(new DetectedThreat
                            {
                                Type = "file",
                                Path = item.FullName,
                                Match = cheatFile,
                                Hash = FileAnalyzer.GetFileHash(item.FullName) ?? "N/A",
                                ExactMatch = true,
                                Risk = "medium"
                            });
                            foundMatch = true;
                            break;
                        }
                    }

                    if (!foundMatch)
                    {
                        foreach (var cheatSig in _cheatDb.CheatProcesses)
                        {
                            if (nameLower.Contains(cheatSig.ToLower()))
                            {
                                found.Add(new DetectedThreat
                                {
                                    Type = "file",
                                    Path = item.FullName,
                                    Match = cheatSig,
                                    Hash = FileAnalyzer.GetFileHash(item.FullName) ?? "N/A",
                                    ExactMatch = false,
                                    Risk = "medium"
                                });
                                break;
                            }
                        }
                    }

                    // Быстрый анализ DLL/EXE (только малые файлы)
                    if ((item.Extension.ToLower() == ".dll" || item.Extension.ToLower() == ".exe") &&
                        item.Length < 2 * 1024 * 1024)
                    {
                        var (score, indicators) = FileAnalyzer.AnalyzeDllFile(item.FullName);
                        if (score >= 5)
                        {
                            found.Add(new DetectedThreat
                            {
                                Type = "file",
                                Path = item.FullName,
                                Match = $"Analysis: {score}/10",
                                Hash = FileAnalyzer.GetFileHash(item.FullName) ?? "N/A",
                                AnalysisScore = score,
                                Indicators = indicators,
                                Risk = score >= 8 ? "high" : "medium"
                            });
                        }
                    }

                    // Анализ конфигов
                    if (item.Extension.ToLower() is ".json" or ".ini" or ".cfg")
                    {
                        var (score, indicators) = FileAnalyzer.AnalyzeConfigFile(item.FullName);
                        if (score >= 5)
                        {
                            found.Add(new DetectedThreat
                            {
                                Type = "file",
                                Path = item.FullName,
                                Match = $"Config: {score}/10",
                                Hash = FileAnalyzer.GetFileHash(item.FullName) ?? "N/A",
                                AnalysisScore = score,
                                Indicators = indicators,
                                Risk = score >= 8 ? "high" : "medium"
                            });
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Пропускаем недоступные файлы
                    _logger.Debug($"Нет доступа к файлу при сканировании: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Пропускаем ошибки
                    _logger.Debug($"Ошибка при обработке файла: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            // Игнорируем ошибки
            _logger.Warning($"Ошибка при сканировании директории: {ex.Message}");
        }

        return found;
    }

    /// <summary>
    /// Проверить систему.
    /// </summary>
    public int CheckSystem()
    {
        var allFound = new List<DetectedThreat>();

        foreach (var suspPath in _cheatDb.SuspiciousPaths)
        {
            _ = AnalyzeDirectoryHeuristic(suspPath);
            allFound.AddRange(CheckDirectory(suspPath));
        }

        // Проверка ProgramData на Windows
        if (OperatingSystem.IsWindows())
        {
            var programData = Environment.GetEnvironmentVariable("PROGRAMDATA") ?? @"C:\ProgramData";
            _ = AnalyzeDirectoryHeuristic(programData);
            allFound.AddRange(CheckDirectory(programData));
        }

        FoundCheats.AddRange(allFound);

        return allFound.Count;
    }
}
