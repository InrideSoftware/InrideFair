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

    /// <summary>
    /// Глубокая проверка DNS-кэша (шумная, по умолчанию выключена).
    /// </summary>
    public bool DeepScanDns { get; init; }

    /// <summary>
    /// Проверка Windows Prefetch.
    /// </summary>
    public bool DeepScanPrefetch { get; init; } = true;
}
