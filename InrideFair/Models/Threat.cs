namespace InrideFair.Models;

/// <summary>
/// Уровень риска угрозы.
/// </summary>
public enum RiskLevel
{
    High,
    Medium,
    Low
}

/// <summary>
/// Тип проверки.
/// </summary>
public enum CheckType
{
    Process,
    File,
    Archive,
    BrowserSearch,
    BrowserDownload,
    BrowserCache,
    BrowserLog,
    BrowserSession,
    DnsCache,
    HostsFile,
    Prefetch,
    Registry,
    Autostart
}

/// <summary>
/// Базовая модель угрозы.
/// </summary>
public record ThreatBase(
    CheckType Type,
    RiskLevel Risk,
    string Match,
    List<string>? Indicators = null
);

/// <summary>
/// Угроза в виде процесса.
/// </summary>
public record ProcessThreat(
    string Name,
    string Signature,
    RiskLevel Risk = RiskLevel.High,
    string Match = "",
    List<string>? Indicators = null
) : ThreatBase(CheckType.Process, Risk, string.IsNullOrEmpty(Match) ? Name : Match, Indicators)
{
    public ProcessThreat(string name, string signature) 
        : this(name, signature, RiskLevel.High, name, null) { }
}

/// <summary>
/// Угроза в виде файла.
/// </summary>
public record FileThreat(
    string Path,
    string Match,
    string? Hash = null,
    int? AnalysisScore = null,
    RiskLevel Risk = RiskLevel.Medium,
    List<string>? Indicators = null
) : ThreatBase(CheckType.File, Risk, Match, Indicators);

/// <summary>
/// Угроза в виде архива.
/// </summary>
public record ArchiveThreat(
    string ArchivePath,
    string FilePath,
    string Match,
    RiskLevel Risk = RiskLevel.Medium
) : ThreatBase(CheckType.Archive, Risk, Match);

/// <summary>
/// Угроза в поиске браузера.
/// </summary>
public record BrowserSearchThreat(
    string Browser,
    string? Url = null,
    string? Title = null,
    string Match = "",
    RiskLevel Risk = RiskLevel.Medium,
    string? VisitDate = null
) : ThreatBase(CheckType.BrowserSearch, Risk, Match)
{
    public BrowserSearchThreat(string browser, string url, string title, string match, string? visitDate = null)
        : this(browser, url, title, match, RiskLevel.Medium, visitDate) { }
};

/// <summary>
/// Угроза в загрузках браузера.
/// </summary>
public record BrowserDownloadThreat(
    string Browser,
    string Filepath,
    string Match,
    RiskLevel Risk = RiskLevel.High,
    string? DownloadDate = null
) : ThreatBase(CheckType.BrowserDownload, Risk, Match);

/// <summary>
/// Угроза в кэше браузера.
/// </summary>
public record BrowserCacheThreat(
    string Browser,
    string Url,
    string CacheFile,
    string Match,
    RiskLevel Risk = RiskLevel.Medium
) : ThreatBase(CheckType.BrowserCache, Risk, Match);

/// <summary>
/// Угроза в логе браузера.
/// </summary>
public record BrowserLogThreat(
    string Browser,
    string Url,
    string LogFile,
    int Line,
    string Match,
    RiskLevel Risk = RiskLevel.Medium
) : ThreatBase(CheckType.BrowserLog, Risk, Match);

/// <summary>
/// Угроза в сессии браузера.
/// </summary>
public record BrowserSessionThreat(
    string Url,
    string SessionFile,
    string Match,
    RiskLevel Risk = RiskLevel.Medium
) : ThreatBase(CheckType.BrowserSession, Risk, Match);

/// <summary>
/// Угроза в DNS кэше.
/// </summary>
public record DnsCacheThreat(
    string Url,
    string Match,
    RiskLevel Risk = RiskLevel.Low
) : ThreatBase(CheckType.DnsCache, Risk, Match);

/// <summary>
/// Угроза в hosts файле.
/// </summary>
public record HostsFileThreat(
    string Url,
    string Match,
    RiskLevel Risk = RiskLevel.High
) : ThreatBase(CheckType.HostsFile, Risk, Match);

/// <summary>
/// Угроза в Prefetch.
/// </summary>
public record PrefetchThreat(
    string File,
    string PrefetchFile,
    string Match,
    RiskLevel Risk = RiskLevel.High
) : ThreatBase(CheckType.Prefetch, Risk, Match);

/// <summary>
/// Угроза в реестре.
/// </summary>
public record RegistryThreat(
    string Hive,
    string Path,
    string? Name = null,
    string? Value = null,
    string Match = "",
    RiskLevel Risk = RiskLevel.High
) : ThreatBase(CheckType.Registry, Risk, Match);

/// <summary>
/// Угроза в автозагрузке (Linux/macOS).
/// </summary>
public record AutostartThreat(
    string Path,
    string Match,
    RiskLevel Risk = RiskLevel.High
) : ThreatBase(CheckType.Autostart, Risk, Match);
