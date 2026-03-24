using System.IO;
using InrideFair.Database;
using InrideFair.Models;
using Microsoft.Win32;

namespace InrideFair.Checkers;

/// <summary>
/// Проверка реестра/автозагрузки на наличие читов.
/// </summary>
public class RegistryChecker
{
    private readonly CheatDatabase _cheatDb;
    public List<DetectedThreat> FoundCheats { get; } = [];
    private readonly string _osName;

    public RegistryChecker(CheatDatabase cheatDb)
    {
        _cheatDb = cheatDb;
        _osName = OperatingSystem.IsWindows() ? "Windows" : 
                  OperatingSystem.IsMacOS() ? "Darwin" : "Linux";
    }

    /// <summary>
    /// Проверить ключ реестра или директорию.
    /// </summary>
    public List<DetectedThreat> CheckRegistryKey(object hiveOrPath, string? keyPath = null)
    {
        var found = new List<DetectedThreat>();

        if (_osName == "Windows")
        {
            try
            {
                if (hiveOrPath is RegistryKeyInfo keyInfo)
                {
                    using var baseKey = RegistryKey.OpenBaseKey(keyInfo.Hive, RegistryView.Registry64);
                    using var key = baseKey.OpenSubKey(keyInfo.Path, false);
                    
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            var value = key.GetValue(valueName);
                            var valueStr = value?.ToString()?.ToLower() ?? "";
                            var nameLower = valueName.ToLower();

                            foreach (var cheatSig in _cheatDb.CheatProcesses)
                            {
                                if (valueStr.Contains(cheatSig.ToLower()) || nameLower.Contains(cheatSig.ToLower()))
                                {
                                    if (!IsLegitimateGame(valueStr))
                                    {
                                        found.Add(new DetectedThreat
                                        {
                                            Type = "registry",
                                            Hive = keyInfo.Hive == RegistryHive.LocalMachine ? "HKLM" : "HKCU",
                                            Path = keyInfo.Path,
                                            ValueName = valueName,
                                            ValueData = value?.ToString() ?? "",
                                            Match = cheatSig,
                                            Risk = "high"
                                        });
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки реестра
            }
        }
        else
        {
            // Проверка директорий автозагрузки для Linux/macOS
            if (hiveOrPath is string pathStr)
            {
                var path = new DirectoryInfo(pathStr);
                if (path.Exists)
                {
                    try
                    {
                        foreach (var item in path.EnumerateFiles())
                        {
                            try
                            {
                                var content = File.ReadAllText(item.FullName).ToLower();
                                foreach (var cheatSig in _cheatDb.CheatProcesses)
                                {
                                    if (content.Contains(cheatSig.ToLower()))
                                    {
                                        found.Add(new DetectedThreat
                                        {
                                            Type = "autostart",
                                            Path = item.FullName,
                                            Match = cheatSig,
                                            Risk = "high"
                                        });
                                        break;
                                    }
                                }
                            }
                            catch
                            {
                                // Пропускаем ошибки чтения
                            }
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки
                    }
                }
            }
        }

        return found;
    }

    private bool IsLegitimateGame(string valueStr)
    {
        return Config.CheatSignatures.LegitimateGamePaths.Any(legit => valueStr.Contains(legit.ToLower()));
    }

    /// <summary>
    /// Проверить систему.
    /// </summary>
    public int CheckSystem()
    {
        var allFound = new List<DetectedThreat>();

        foreach (var item in _cheatDb.RegistryKeys)
        {
            if (_osName == "Windows")
            {
                allFound.AddRange(CheckRegistryKey(item));
            }
            else
            {
                allFound.AddRange(CheckRegistryKey(item));
            }
        }

        FoundCheats.AddRange(allFound);

        return allFound.Count;
    }
}
