using System.IO;
using InrideFair.Database;
using InrideFair.Services;
using InrideFair.Utils;

namespace InrideFair.Checkers;

/// <summary>
/// Проверка архивов на наличие читов.
/// </summary>
public class ArchiveChecker
{
    private readonly CheatDatabase _cheatDb;
    public List<Dictionary<string, object?>> FoundCheats { get; } = [];

    public ArchiveChecker(CheatDatabase cheatDb)
    {
        _cheatDb = cheatDb;
    }

    /// <summary>
    /// Найти архивы в директории.
    /// </summary>
    public List<string> FindArchives(string directory)
    {
        var archives = new List<string>();
        try
        {
            var dir = new DirectoryInfo(directory);
            if (!dir.Exists)
                return archives;

            foreach (var item in dir.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    if (_cheatDb.ArchiveExtensions.Contains(item.Extension.ToLower()))
                        archives.Add(item.FullName);
                }
                catch (UnauthorizedAccessException)
                {
                    // Пропускаем недоступные файлы
                }
            }
        }
        catch (Exception ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning($"Ошибка при проверке архивов в {directory}", ex);
        }
        return archives;
    }

    /// <summary>
    /// Проверить содержимое архива.
    /// </summary>
    public List<Dictionary<string, object?>> CheckArchiveContents(string archivePath)
    {
        var found = new List<Dictionary<string, object?>>();
        var extractDir = ArchiveExtractor.ExtractArchive(archivePath);

        if (extractDir == null)
            return found;

        try
        {
            var extractInfo = new DirectoryInfo(extractDir);
            foreach (var item in extractInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    var nameLower = item.Name.ToLower();

                    foreach (var cheatFile in _cheatDb.CheatFiles)
                    {
                        if (nameLower == cheatFile.ToLower())
                        {
                            found.Add(new Dictionary<string, object?>
                            {
                                ["path"] = item.FullName,
                                ["match"] = cheatFile,
                                ["archive"] = archivePath,
                                ["exact_match"] = true
                            });
                            break;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Пропускаем недоступные файлы
                }
                catch (Exception)
                {
                    // Пропускаем ошибки
                }
            }
        }
        catch (Exception)
        {
            // Игнорируем ошибки
        }
        finally
        {
            try
            {
                Directory.Delete(extractDir, true);
            }
            catch
            {
                // Игнорируем ошибки удаления
            }
        }

        return found;
    }

    /// <summary>
    /// Проверить архивы в директории.
    /// </summary>
    public int CheckArchivesInDirectory(string directory)
    {
        var archives = FindArchives(directory);
        if (archives.Count == 0)
            return 0;

        var totalFound = 0;
        foreach (var archive in archives)
        {
            var found = CheckArchiveContents(archive);
            foreach (var item in found)
            {
                FoundCheats.Add(new Dictionary<string, object?>
                {
                    ["type"] = "archive",
                    ["archive_path"] = item["archive"],
                    ["file_path"] = item["path"],
                    ["match"] = item["match"],
                    ["risk"] = "medium"
                });
                totalFound++;
            }
        }

        return totalFound;
    }
}
