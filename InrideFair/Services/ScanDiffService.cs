using System.IO;
using System.Text.Json;
using InrideFair.Models;

namespace InrideFair.Services;

public record ScanDiffSummary(
    int AddedCount,
    int RemovedCount,
    IReadOnlyList<string> Added,
    IReadOnlyList<string> Removed);

/// <summary>
/// Сравнение двух отчётов сканирования.
/// </summary>
public static class ScanDiffService
{
    public static ScanDiffSummary CompareReports(string? previousReportPath, ScanResultData current)
    {
        if (string.IsNullOrWhiteSpace(previousReportPath) || !File.Exists(previousReportPath))
        {
            return new ScanDiffSummary(0, 0, [], []);
        }

        try
        {
            var json = File.ReadAllText(previousReportPath);
            var previous = JsonSerializer.Deserialize<ScanResultData>(json);
            if (previous == null)
                return new ScanDiffSummary(0, 0, [], []);

            var previousKeys = CollectThreatKeys(previous).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var currentKeys = CollectThreatKeys(current).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var added = currentKeys.Except(previousKeys, StringComparer.OrdinalIgnoreCase).Order().ToList();
            var removed = previousKeys.Except(currentKeys, StringComparer.OrdinalIgnoreCase).Order().ToList();

            return new ScanDiffSummary(added.Count, removed.Count, added, removed);
        }
        catch
        {
            return new ScanDiffSummary(0, 0, [], []);
        }
    }

    private static IEnumerable<string> CollectThreatKeys(ScanResultData data)
    {
        foreach (var threat in data.Processes.Concat(data.Files).Concat(data.Archives).Concat(data.Browser).Concat(data.Registry))
        {
            yield return FormatThreatKey(threat);
        }
    }

    private static string FormatThreatKey(ThreatData threat)
    {
        var location = !string.IsNullOrWhiteSpace(threat.Path) ? threat.Path
            : !string.IsNullOrWhiteSpace(threat.FilePath) ? threat.FilePath
            : !string.IsNullOrWhiteSpace(threat.Url) ? threat.Url
            : threat.Name;

        return $"[{threat.Type}] {location} ({threat.Match})";
    }
}
