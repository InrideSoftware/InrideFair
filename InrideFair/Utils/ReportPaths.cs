using System.IO;

namespace InrideFair.Utils;

/// <summary>
/// Имена и пути файлов отчётов.
/// </summary>
public static class ReportPaths
{
    public const string LatestJsonName = "inridefair_report.json";
    public const string LatestHtmlName = "inridefair_report.html";

    public static string GenerateBaseName(DateTime? timestamp = null)
    {
        var value = timestamp ?? DateTime.Now;
        return $"inridefair_report_{value:yyyy-MM-dd_HH-mm-ss}";
    }

    public static string GetTimestampedJsonPath(string baseDirectory, DateTime? timestamp = null) =>
        Path.Combine(baseDirectory, $"{GenerateBaseName(timestamp)}.json");

    public static string GetTimestampedHtmlPath(string baseDirectory, DateTime? timestamp = null) =>
        Path.Combine(baseDirectory, $"{GenerateBaseName(timestamp)}.html");

    public static string? FindPreviousReportPath(string baseDirectory, string currentReportPath)
    {
        if (!Directory.Exists(baseDirectory))
            return null;

        return Directory.EnumerateFiles(baseDirectory, "inridefair_report*.json")
            .Where(path => !string.Equals(path, currentReportPath, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }
}
