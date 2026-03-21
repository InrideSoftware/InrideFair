using System.IO;
using InrideFair.Config;
using InrideFair.Services;
using Microsoft.Win32;

namespace InrideFair.Database;

/// <summary>
/// База данных сигнатур читов.
/// </summary>
public class CheatDatabase
{
    private readonly AppConfig _config;

    public List<string> CustomExclusions { get; }
    public List<string> CustomSignatures { get; }
    public List<string> CheatProcesses { get; }
    public List<string> CheatFiles { get; }
    public List<string> ArchiveExtensions { get; }
    public List<string> SystemFiles { get; }
    public List<string> SuspiciousPaths { get; }
    public List<object> RegistryKeys { get; }

    public CheatDatabase(AppConfig? config = null)
    {
        _config = config ?? ConfigValidator.ValidateAndLoad();

        CustomExclusions = _config.CustomExclusions;
        CustomSignatures = _config.CustomSignatures;
        CheatProcesses = [.. CheatSignatures.CheatProcesses, .. CustomSignatures];
        CheatFiles = [.. CheatSignatures.CheatFiles];
        ArchiveExtensions = [.. CheatSignatures.ArchiveExtensions];
        SystemFiles = [.. CheatSignatures.SystemFiles];
        SuspiciousPaths = GetSuspiciousPaths();
        RegistryKeys = GetRegistryKeys();
    }

    /// <summary>
    /// Получить подозрительные пути для текущей ОС.
    /// </summary>
    private List<string> GetSuspiciousPaths()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (OperatingSystem.IsWindows())
        {
            return
            [
                Path.Combine(home, "AppData", "Local", "Temp"),
                Path.Combine(home, "AppData", "Roaming"),
                Path.Combine(home, "Downloads")
            ];
        }

        if (OperatingSystem.IsMacOS())
        {
            return
            [
                Path.Combine(home, "Library", "Caches"),
                Path.Combine(home, "Library", "Application Support"),
                Path.Combine(home, "Downloads")
            ];
        }

        // Linux
        return
        [
            "/tmp",
            Path.Combine(home, ".cache"),
            Path.Combine(home, ".config"),
            Path.Combine(home, "Downloads")
        ];
    }

    /// <summary>
    /// Получить ключи реестра/конфигурации для текущей ОС.
    /// </summary>
    private List<object> GetRegistryKeys()
    {
        if (OperatingSystem.IsWindows())
        {
            return
            [
                new RegistryKeyInfo(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"),
                new RegistryKeyInfo(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"),
                new RegistryKeyInfo(RegistryHive.CurrentUser, @"SOFTWARE"),
                new RegistryKeyInfo(RegistryHive.LocalMachine, @"SOFTWARE")
            ];
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (OperatingSystem.IsMacOS())
        {
            return
            [
                Path.Combine(home, "Library", "LaunchAgents"),
                "/Library/LaunchAgents",
                "/Library/LaunchDaemons"
            ];
        }

        // Linux autostart
        return
        [
            Path.Combine(home, ".config", "autostart"),
            "/etc/xdg/autostart",
            "/usr/share/applications"
        ];
    }

    /// <summary>
    /// Проверить, исключён ли путь.
    /// </summary>
    public bool IsExcluded(string path)
    {
        var pathLower = path.ToLower();
        return CustomExclusions.Any(e => 
            e.ToLower().Contains(pathLower) || pathLower.Contains(e.ToLower()));
    }

    /// <summary>
    /// Проверить, является ли файл системным.
    /// </summary>
    public bool IsSystemFile(string path)
    {
        var pathLower = path.ToLower();
        return SystemFiles.Any(sf => pathLower.Contains(sf.ToLower()));
    }

    /// <summary>
    /// Проверить, является ли путь легитимной игровой директорией.
    /// </summary>
    public bool IsLegitimateGamePath(string path)
    {
        var pathLower = path.ToLower();
        return CheatSignatures.LegitimateGamePaths.Any(legit => pathLower.Contains(legit.ToLower()));
    }
}

/// <summary>
/// Информация о ключе реестра.
/// </summary>
public record RegistryKeyInfo(RegistryHive Hive, string Path);
