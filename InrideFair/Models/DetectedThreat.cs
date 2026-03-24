namespace InrideFair.Models;

/// <summary>
/// Унифицированная типизированная модель найденной угрозы.
/// Поля заполняются по мере доступности в конкретном чекере.
/// </summary>
public record DetectedThreat
{
    public string Type { get; init; } = "";
    public string Name { get; init; } = "";
    public string Signature { get; init; } = "";
    public string Path { get; init; } = "";
    public string ArchivePath { get; init; } = "";
    public string FilePath { get; init; } = "";
    public string Match { get; init; } = "";
    public string Hash { get; init; } = "";
    public string Risk { get; init; } = "medium";
    public int? AnalysisScore { get; init; }
    public List<string> Indicators { get; init; } = [];
    public string Browser { get; init; } = "";
    public string Url { get; init; } = "";
    public string Title { get; init; } = "";
    public string Hive { get; init; } = "";
    public string KeyPath { get; init; } = "";
    public string ValueName { get; init; } = "";
    public string ValueData { get; init; } = "";
    public bool ExactMatch { get; init; }
}
