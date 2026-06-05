using System.IO;
using InrideFair.Database;
using InrideFair.Models;
using InrideFair.Services;
using InrideFair.Utils;

namespace InrideFair.Checkers;

/// <summary>
/// Проверка архивов на наличие читов.
/// </summary>
public class ArchiveChecker
{
    private readonly CheatDatabase _cheatDb;
    private readonly ILoggingService _logger;
    public List<DetectedThreat> FoundCheats { get; } = [];

    public ArchiveChecker(CheatDatabase cheatDb, ILoggingService logger)
    {
        _cheatDb = cheatDb;
        _logger = logger;
    }

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
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Ошибка при проверке архивов в {directory}", ex);
        }

        return archives;
    }

    public List<DetectedThreat> CheckArchiveContents(string archivePath)
    {
        var found = new List<DetectedThreat>();
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

                    if (ReportFileDetector.IsReportFilePath(item.FullName))
                        continue;

                    foreach (var cheatFile in _cheatDb.CheatFiles)
                    {
                        if (nameLower == cheatFile.ToLower())
                        {
                            found.Add(new DetectedThreat
                            {
                                Type = "archive",
                                Path = item.FullName,
                                FilePath = item.FullName,
                                ArchivePath = archivePath,
                                Match = cheatFile,
                                ExactMatch = true,
                                Risk = "medium"
                            });
                            break;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            try
            {
                Directory.Delete(extractDir, true);
            }
            catch
            {
            }
        }

        return found;
    }

    public int CheckArchivesInDirectory(string directory, CancellationToken cancellationToken = default)
    {
        var archives = FindArchives(directory);
        if (archives.Count == 0)
            return 0;

        var totalFound = 0;
        foreach (var archive in archives)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var found = CheckArchiveContents(archive);
            foreach (var item in found)
            {
                FoundCheats.Add(item);
                totalFound++;
            }
        }

        return totalFound;
    }
}
