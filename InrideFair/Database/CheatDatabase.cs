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
    private AppConfig _config;

    public List<string> CustomExclusions { get; } = [];
    public List<string> CustomSignatures { get; } = [];
    public List<string> CheatProcesses { get; } = [];
    public List<string> CheatFiles { get; } = [];
    public List<string> ArchiveExtensions { get; } = [];
    public List<string> SystemFiles { get; } = [];
    public List<string> SuspiciousPaths { get; private set; } = [];
    public List<object> RegistryKeys { get; private set; } = [];

    public AppConfig Settings => _config;

    public CheatDatabase(AppConfig? config = null)
    {
        _config = config ?? ConfigValidator.ValidateAndLoad();
        ReloadSettings(_config);
    }

    public void ReloadSettings(AppConfig? config = null)
    {
        _config = config ?? ConfigValidator.ValidateAndLoad();

        CustomExclusions.Clear();
        CustomExclusions.AddRange(_config.CustomExclusions);

        CustomSignatures.Clear();
        CustomSignatures.AddRange(_config.CustomSignatures);

        var external = SignatureLoader.Load();

        CheatProcesses.Clear();
        CheatProcesses.AddRange(CheatSignatures.CheatProcesses);
        CheatProcesses.AddRange(external.Processes);
        CheatProcesses.AddRange(CustomSignatures);

        CheatFiles.Clear();
        CheatFiles.AddRange(CheatSignatures.CheatFiles);
        CheatFiles.AddRange(external.Files);

        ArchiveExtensions.Clear();
        ArchiveExtensions.AddRange(CheatSignatures.ArchiveExtensions);

        SystemFiles.Clear();
        SystemFiles.AddRange(CheatSignatures.SystemFiles);

        SuspiciousPaths = GetSuspiciousPaths();
        RegistryKeys = GetRegistryKeys();
    }

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

        return
        [
            "/tmp",
            Path.Combine(home, ".cache"),
            Path.Combine(home, ".config"),
            Path.Combine(home, "Downloads")
        ];
    }

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

        return
        [
            Path.Combine(home, ".config", "autostart"),
            "/etc/xdg/autostart",
            "/usr/share/applications"
        ];
    }

    public bool IsExcluded(string path)
    {
        var pathLower = path.ToLowerInvariant();
        return CustomExclusions.Any(e =>
            e.ToLowerInvariant().Contains(pathLower) || pathLower.Contains(e.ToLowerInvariant()));
    }

    public bool IsSystemFile(string path)
    {
        var pathLower = path.ToLowerInvariant();
        return SystemFiles.Any(sf => pathLower.Contains(sf.ToLowerInvariant()));
    }

    public bool IsLegitimateGamePath(string path)
    {
        var pathLower = path.ToLowerInvariant();
        return CheatSignatures.LegitimateGamePaths.Any(legit => pathLower.Contains(legit.ToLowerInvariant()));
    }
}

public record RegistryKeyInfo(RegistryHive Hive, string Path);
