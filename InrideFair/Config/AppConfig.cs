namespace InrideFair.Config;

/// <summary>
/// Конфигурация приложения (модель данных).
/// </summary>
public record AppConfig
{
    /// <summary>
    /// Пользовательские исключения (пути).
    /// </summary>
    public List<string> CustomExclusions { get; init; } =
    [
        "C:\\Program Files",
        "C:\\Program Files (x86)",
        "C:\\Windows"
    ];

    /// <summary>
    /// Пользовательские сигнатуры.
    /// </summary>
    public List<string> CustomSignatures { get; init; } = [];
}
