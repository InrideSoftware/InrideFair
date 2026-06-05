using System.IO;
using System.Text.Json;

namespace InrideFair.Utils;

/// <summary>
/// Определение собственных отчётов Inride Fair, чтобы не сканировать их как читы.
/// </summary>
public static class ReportFileDetector
{
    public static bool IsReportFilePath(string filePath)
    {
        var name = Path.GetFileName(filePath);
        if (string.IsNullOrEmpty(name))
            return false;

        var nameLower = name.ToLowerInvariant();
        if (!nameLower.StartsWith("inridefair_report", StringComparison.Ordinal))
            return false;

        return nameLower.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
               nameLower.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsReportJsonContent(string content)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (!root.TryGetProperty("Version", out _) ||
                !root.TryGetProperty("ScanDate", out _) ||
                !root.TryGetProperty("Summary", out _))
            {
                return false;
            }

            return root.TryGetProperty("Processes", out _) ||
                   root.TryGetProperty("Files", out _) ||
                   root.TryGetProperty("Browser", out _) ||
                   root.TryGetProperty("Registry", out _) ||
                   root.TryGetProperty("Archives", out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static bool IsReportHtmlContent(string content) =>
        content.Contains("Inride Fair — Отчёт о сканировании", StringComparison.Ordinal) ||
        content.Contains("Inride Fair v", StringComparison.Ordinal) &&
        content.Contains("Отчёт о сканировании", StringComparison.Ordinal);

    public static bool ShouldSkipFile(string filePath)
    {
        if (IsReportFilePath(filePath))
            return true;

        try
        {
            if (filePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                var head = File.ReadAllText(filePath);
                if (IsReportHtmlContent(head))
                    return true;
            }

            if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                return false;

            var content = File.ReadAllText(filePath);
            return IsReportJsonContent(content);
        }
        catch
        {
            return false;
        }
    }
}
