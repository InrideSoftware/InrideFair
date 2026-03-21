using System.IO;
using InrideFair.Database;
using Microsoft.Win32;

namespace InrideFair.Checkers;

/// <summary>
/// Проверка реестра/автозагрузки на наличие читов.
/// </summary>
public class RegistryChecker
{
    private readonly CheatDatabase _cheatDb;
    public List<Dictionary<string, object?>> FoundCheats { get; } = [];
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
    public List<Dictionary<string, object?>> CheckRegistryKey(object hiveOrPath, string? keyPath = null)
    {
        var found = new List<Dictionary<string, object?>>();

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
                                        found.Add(new Dictionary<string, object?>
                                        {
                                            ["hive"] = keyInfo.Hive == RegistryHive.LocalMachine ? "HKLM" : "HKCU",
                                            ["path"] = keyInfo.Path,
                                            ["name"] = valueName,
                                            ["value"] = value?.ToString(),
                                            ["match"] = cheatSig
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
                                        found.Add(new Dictionary<string, object?>
                                        {
                                            ["path"] = item.FullName,
                                            ["match"] = cheatSig,
                                            ["type"] = "autostart"
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
        var allFound = new List<Dictionary<string, object?>>();

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

        foreach (var item in allFound)
        {
            if (_osName == "Windows")
            {
                FoundCheats.Add(new Dictionary<string, object?>
                {
                    ["type"] = "registry",
                    ["hive"] = item["hive"],
                    ["path"] = item["path"],
                    ["name"] = item["name"],
                    ["value"] = item["value"],
                    ["match"] = item["match"],
                    ["risk"] = "high"
                });
            }
            else
            {
                FoundCheats.Add(new Dictionary<string, object?>
                {
                    ["type"] = "autostart",
                    ["path"] = item["path"],
                    ["match"] = item["match"],
                    ["risk"] = "high"
                });
            }
        }

        return allFound.Count;
    }
}
