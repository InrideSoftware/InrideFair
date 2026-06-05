using InrideFair.Models;
using InrideFair.Scanner;

namespace InrideFair.Services;

/// <summary>
/// Преобразование и агрегация моделей угроз.
/// </summary>
public static class ThreatMapper
{
    public static ThreatData ToThreatData(DetectedThreat threat) => new()
    {
        Type = threat.Type,
        Name = threat.Name,
        Path = GetDisplayPath(threat),
        ArchivePath = threat.ArchivePath,
        FilePath = threat.FilePath,
        Match = threat.Match,
        Hash = threat.Hash,
        Risk = threat.Risk,
        AnalysisScore = threat.AnalysisScore,
        Indicators = threat.Indicators,
        Browser = threat.Browser,
        Url = threat.Url,
        Title = threat.Title,
        Hive = threat.Hive,
        KeyPath = threat.KeyPath,
        ValueName = threat.ValueName,
        ValueData = threat.ValueData,
        ExactMatch = threat.ExactMatch
    };

    public static (int Total, int High, int Medium, int Low) CountByRisk(IEnumerable<DetectedThreat> threats)
    {
        var high = 0;
        var medium = 0;
        var low = 0;

        foreach (var threat in threats)
        {
            switch (threat.Risk.ToLowerInvariant())
            {
                case "high":
                    high++;
                    break;
                case "low":
                    low++;
                    break;
                default:
                    medium++;
                    break;
            }
        }

        return (high + medium + low, high, medium, low);
    }

    public static (int Processes, int Files, int Archives, int Browser, int Registry) CountByCategory(ScanResult result) =>
        (result.Processes.Count, result.Files.Count, result.Archives.Count, result.Browser.Count, result.Registry.Count);

    private static string GetDisplayPath(DetectedThreat threat)
    {
        if (!string.IsNullOrEmpty(threat.Path))
            return threat.Path;

        if (!string.IsNullOrEmpty(threat.FilePath))
            return threat.FilePath;

        if (!string.IsNullOrEmpty(threat.Url))
            return threat.Url;

        if (!string.IsNullOrEmpty(threat.Name))
            return threat.Name;

        return threat.KeyPath;
    }
}
