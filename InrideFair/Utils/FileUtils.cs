using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using InrideFair.Services;

namespace InrideFair.Utils;

/// <summary>
/// Утилиты для работы с процессами.
/// </summary>
public static class ProcessUtils
{
    /// <summary>
    /// Получить список запущенных процессов.
    /// </summary>
    public static List<string> GetRunningProcesses()
    {
        try
        {
            return Process.GetProcesses()
                .Select(p => p.ProcessName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }
        catch (Exception)
        {
            return [];
        }
    }

    /// <summary>
    /// Проверить права администратора.
    /// </summary>
    public static bool IsAdmin()
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            try
            {
                return UnixFileSystemUtils.GetEuid() == 0;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Открыть папку с файлом в проводнике.
    /// </summary>
    public static bool OpenFolder(string filepath)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{filepath}\"",
                    UseShellExecute = true
                });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = $"-R \"{filepath}\"",
                    UseShellExecute = true
                });
            }
            else // Linux
            {
                var directory = Path.GetDirectoryName(filepath);
                if (directory != null)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = $"\"{directory}\"",
                        UseShellExecute = true
                    });
                }
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

/// <summary>
/// Утилиты для Unix-систем.
/// </summary>
internal static class UnixFileSystemUtils
{
    [System.Runtime.InteropServices.DllImport("libc", EntryPoint = "geteuid", SetLastError = false)]
    private static extern uint GetEuidUnix();

    public static uint GetEuid()
    {
        try
        {
            return GetEuidUnix();
        }
        catch
        {
            return uint.MaxValue;
        }
    }
}

/// <summary>
/// Утилиты для анализа файлов.
/// </summary>
public static class FileAnalyzer
{
    /// <summary>
    /// Получить MD5 хеш файла.
    /// </summary>
    public static string? GetFileHash(string filepath)
    {
        try
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filepath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning($"Не удалось получить хеш файла: {filepath}", ex);
            return null;
        }
    }

    /// <summary>
    /// Быстрый анализ DLL/EXE файла (макс 5MB).
    /// </summary>
    public static (int score, List<string> indicators) AnalyzeDllFile(string filepath)
    {
        var score = 0;
        var indicators = new List<string>();

        try
        {
            var fileInfo = new FileInfo(filepath);
            if (fileInfo.Length > Config.AnalysisConstants.MaxDllAnalysisSize)
                return (0, indicators);

            // Оптимизация: читаем только первые N байт вместо всего файла
            var maxReadSize = (int)Math.Min(fileInfo.Length, Config.AnalysisConstants.MaxDllReadSize);
            var content = new byte[maxReadSize];
            
            using var stream = File.OpenRead(filepath);
            var bytesRead = stream.Read(content, 0, maxReadSize);
            
            if (bytesRead < maxReadSize)
            {
                Array.Resize(ref content, bytesRead);
            }

            // Проверка строк
            var suspiciousStrings = new[] { "Osiris", "Neverlose", "aimbot", "wallhack", "CS2" };
            foreach (var str in suspiciousStrings)
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                if (ContainsBytes(content, bytes))
                {
                    score += 2;
                    indicators.Add($"String: {str}");
                }
            }

            // Проверка импортов
            var suspiciousImports = new[] { "LoadLibrary", "CreateRemoteThread", "WriteProcessMemory" };
            foreach (var imp in suspiciousImports)
            {
                var bytes = Encoding.UTF8.GetBytes(imp);
                if (ContainsBytes(content, bytes))
                {
                    score += 2;
                    indicators.Add($"Import: {imp}");
                }
            }

            // ImGui
            if (ContainsBytes(content, Encoding.UTF8.GetBytes("ImGui")))
            {
                score += 1;
                indicators.Add("ImGui detected");
            }

            // Подозрительные секции
            var suspiciousSections = new[] { ".cheat", ".hack", ".vmp" };
            foreach (var sec in suspiciousSections)
            {
                var bytes = Encoding.UTF8.GetBytes(sec);
                if (ContainsBytes(content, bytes))
                {
                    score += 5;
                    indicators.Add($"Suspicious: {sec}");
                }
            }
        }
        catch (Exception ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning($"Ошибка при анализе файла: {filepath}", ex);
        }

        return (Math.Min(score, 10), indicators.Take(3).ToList());
    }

    /// <summary>
    /// Анализ конфига на наличие признаков читов.
    /// </summary>
    public static (int score, List<string> indicators) AnalyzeConfigFile(string filepath)
    {
        var score = 0;
        var indicators = new List<string>();

        try
        {
            var content = File.ReadAllText(filepath, Encoding.UTF8);
            var contentLower = content.ToLower();

            // Проверка названий читов
            foreach (var name in Config.CheatSignatures.CheatNames)
            {
                if (contentLower.Contains(name))
                {
                    score += 3;
                    indicators.Add($"Cheat name: {name}");
                }
            }

            // Проверка полей читов
            foreach (var field in Config.CheatSignatures.CheatFields)
            {
                if (contentLower.Contains(field))
                {
                    score += 1;
                    indicators.Add($"Field: {field}");
                }
            }

            // Проверка структуры JSON читов
            if (filepath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    // Проверка на наличие offsets/signatures
                    if (root.TryGetProperty("offsets", out _) || root.TryGetProperty("signatures", out _))
                    {
                        score += 3;
                        indicators.Add("Contains offsets/signatures");
                    }

                    // Проверка на наличие настроек чита
                    var cheatKeys = new[] { "aimbot", "visuals", "misc", "skins", "config" };
                    foreach (var key in cheatKeys)
                    {
                        if (root.TryGetProperty(key, out _))
                        {
                            score += 2;
                            indicators.Add($"JSON key: {key}");
                        }
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // Не JSON или ошибка парсинга
                }
            }

            // Проверка INI файлов
            if (filepath.EndsWith(".ini", StringComparison.OrdinalIgnoreCase))
            {
                var iniSections = new[] { "[Aimbot]", "[Visuals]", "[ESP]", "[Skins]", "[Misc]" };
                foreach (var section in iniSections)
                {
                    if (content.Contains(section, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 3;
                        indicators.Add($"INI section: {section}");
                    }
                }
            }

            // Проверка на наличие хешей/ключей
            var hashPattern = new System.Text.RegularExpressions.Regex(@"[a-fA-F0-9]{32,64}");
            if (hashPattern.IsMatch(content))
            {
                score += 1;
                indicators.Add("Contains hash/key");
            }

            score = Math.Min(score, Config.AnalysisConstants.MaxConfigAnalysisScore);
        }
        catch (Exception)
        {
            // Игнорируем ошибки
        }

        return (score, indicators);
    }

    private static bool ContainsBytes(byte[] haystack, byte[] needle)
    {
        if (needle.Length > haystack.Length)
            return false;

        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    found = false;
                    break;
                }
            }
            if (found)
                return true;
        }
        return false;
    }
}

/// <summary>
/// Утилиты для работы с архивами.
/// </summary>
public static class ArchiveExtractor
{
    /// <summary>
    /// Извлечь архив.
    /// </summary>
    public static string? ExtractArchive(string archivePath, string? extractTo = null)
    {
        extractTo ??= Path.Combine(Path.GetTempPath(), $"cheatcheck_{Path.GetFileNameWithoutExtension(archivePath)}");

        var archiveLower = archivePath.ToLower();

        try
        {
            if (archiveLower.EndsWith(".zip"))
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, extractTo, true);
                return extractTo;
            }

            // Для RAR и 7z пробуем внешние утилиты
            if (archiveLower.EndsWith(".rar"))
            {
                var tools = new[]
                {
                    new[] { "unrar", "x", "-y", archivePath, extractTo },
                    new[] { "/usr/bin/unrar", "x", "-y", archivePath, extractTo }
                };

                if (OperatingSystem.IsWindows())
                {
                    var winTools = new[]
                    {
                        new[] { @"C:\Program Files\WinRAR\WinRAR.exe", "x", "-y", archivePath, extractTo },
                        new[] { @"C:\Program Files (x86)\WinRAR\WinRAR.exe", "x", "-y", archivePath, extractTo }
                    };
                    tools = [.. tools, .. winTools];
                }

                foreach (var tool in tools)
                {
                    try
                    {
                        var result = Process.Start(new ProcessStartInfo
                        {
                            FileName = tool[0],
                            Arguments = string.Join(" ", tool.Skip(1)),
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        });
                        result?.WaitForExit(60000);
                        if (result?.ExitCode == 0)
                            return extractTo;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            if (archiveLower.EndsWith(".7z"))
            {
                var tools = new[]
                {
                    new[] { "7z", "x", $"-o{extractTo}", "-y", archivePath },
                    new[] { "/usr/bin/7z", "x", $"-o{extractTo}", "-y", archivePath }
                };

                if (OperatingSystem.IsWindows())
                {
                    var winTools = new[]
                    {
                        new[] { @"C:\Program Files\7-Zip\7z.exe", "x", $"-o{extractTo}", "-y", archivePath },
                        new[] { @"C:\Program Files (x86)\7-Zip\7z.exe", "x", $"-o{extractTo}", "-y", archivePath }
                    };
                    tools = [.. tools, .. winTools];
                }

                foreach (var tool in tools)
                {
                    try
                    {
                        var result = Process.Start(new ProcessStartInfo
                        {
                            FileName = tool[0],
                            Arguments = string.Join(" ", tool.Skip(1)),
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        });
                        result?.WaitForExit(120000);
                        if (result?.ExitCode == 0)
                            return extractTo;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            if (archiveLower.EndsWith(".tar") || archiveLower.EndsWith(".tar.gz") || 
                archiveLower.EndsWith(".tgz") || archiveLower.EndsWith(".tar.bz2") || 
                archiveLower.EndsWith(".tar.xz"))
            {
                // Для tar архивов используем встроенную поддержку
                // В .NET 8+ можно использовать System.IO.Compression для tar
                // Для простоты пока пропускаем сложные tar архивы
            }
        }
        catch (Exception)
        {
            // Игнорируем ошибки
        }

        return null;
    }
}

/// <summary>
/// Утилиты для работы с файлами.
/// </summary>
public static class FileUtils
{
    /// <summary>
    /// Безопасное копирование файла.
    /// </summary>
    public static bool CopyFileSafe(string src, string dst)
    {
        try
        {
            File.Copy(src, dst, true);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Очистить временные папки.
    /// </summary>
    public static void CleanupTempFolders(string prefix = "cheatcheck_")
    {
        try
        {
            var tempDir = Path.GetTempPath();
            foreach (var folder in Directory.EnumerateDirectories(tempDir, $"{prefix}*"))
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch
                {
                    // Игнорируем ошибки удаления
                }
            }
        }
        catch
        {
            // Игнорируем ошибки
        }
    }
}
