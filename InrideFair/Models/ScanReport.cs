using System.Text.Json.Serialization;

namespace InrideFair.Models;

/// <summary>
/// Информация о системе.
/// </summary>
public record SystemInfo(
    string Os,
    string Version,
    string Machine,
    string Python
);

/// <summary>
/// Результаты проверок по категориям.
/// </summary>
public record CheckResults(
    [property: JsonPropertyName("processes")] List<Dictionary<string, object?>> Processes,
    [property: JsonPropertyName("files")] List<Dictionary<string, object?>> Files,
    [property: JsonPropertyName("registry")] List<Dictionary<string, object?>> Registry,
    [property: JsonPropertyName("archives")] List<Dictionary<string, object?>> Archives,
    [property: JsonPropertyName("browser")] List<Dictionary<string, object?>> Browser
);

/// <summary>
/// Сводка результатов.
/// </summary>
public record Summary(
    [property: JsonPropertyName("total_threats")] int TotalThreats = 0,
    [property: JsonPropertyName("high_risk")] int HighRisk = 0,
    [property: JsonPropertyName("medium_risk")] int MediumRisk = 0
);

/// <summary>
/// Отчёт о сканировании.
/// </summary>
public record ScanReport(
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("system")] SystemInfo System,
    [property: JsonPropertyName("computer_name")] string ComputerName,
    [property: JsonPropertyName("checks")] CheckResults Checks,
    [property: JsonPropertyName("summary")] Summary Summary
)
{
    /// <summary>
    /// Создать пустой отчёт.
    /// </summary>
    public static ScanReport CreateEmpty(SystemInfo systemInfo)
    {
        return new ScanReport(
            Timestamp: DateTime.Now.ToString("O"),
            System: systemInfo,
            ComputerName: systemInfo.Os.Split(' ')[0],
            Checks: new CheckResults(
                Processes: new List<Dictionary<string, object?>>(),
                Files: new List<Dictionary<string, object?>>(),
                Registry: new List<Dictionary<string, object?>>(),
                Archives: new List<Dictionary<string, object?>>(),
                Browser: new List<Dictionary<string, object?>>()
            ),
            Summary: new Summary()
        );
    }

    /// <summary>
    /// Обновить сводку на основе результатов.
    /// </summary>
    public ScanReport WithUpdatedSummary()
    {
        var allCheats = Checks.Processes
            .Concat(Checks.Files)
            .Concat(Checks.Registry)
            .Concat(Checks.Archives)
            .Concat(Checks.Browser)
            .ToList();

        return this with
        {
            Summary = new Summary(
                TotalThreats: allCheats.Count,
                HighRisk: allCheats.Count(c => c.TryGetValue("risk", out var r) && r?.ToString() == "high"),
                MediumRisk: allCheats.Count(c => c.TryGetValue("risk", out var r) && r?.ToString() == "medium")
            )
        };
    }
}
