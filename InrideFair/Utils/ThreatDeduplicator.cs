using InrideFair.Models;

namespace InrideFair.Utils;

/// <summary>
/// Удаление дубликатов находок по типу, ключу и совпадению.
/// </summary>
public static class ThreatDeduplicator
{
    public static List<DetectedThreat> Deduplicate(IEnumerable<DetectedThreat> threats)
    {
        return threats
            .GroupBy(GetKey, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    private static string GetKey(DetectedThreat threat)
    {
        var identity = !string.IsNullOrEmpty(threat.Path) ? threat.Path
            : !string.IsNullOrEmpty(threat.FilePath) ? threat.FilePath
            : !string.IsNullOrEmpty(threat.Url) ? threat.Url
            : !string.IsNullOrEmpty(threat.Name) ? threat.Name
            : threat.KeyPath;

        return $"{threat.Type}|{identity}|{threat.Match}".ToLowerInvariant();
    }
}
