using System.Diagnostics;
using InrideFair.Database;
using InrideFair.Models;
using InrideFair.Utils;

namespace InrideFair.Checkers;

/// <summary>
/// Проверка запущенных процессов на наличие читов.
/// </summary>
public class ProcessChecker
{
    private readonly CheatDatabase _cheatDb;
    public List<DetectedThreat> FoundCheats { get; } = [];

    public ProcessChecker(CheatDatabase cheatDb)
    {
        _cheatDb = cheatDb;
    }

    /// <summary>
    /// Проверить процессы.
    /// </summary>
    public int CheckProcesses(CancellationToken cancellationToken = default)
    {
        var checkedCount = 0;

        foreach (var process in Process.GetProcesses())
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (process)
            {
                var processName = process.ProcessName;
                if (string.IsNullOrEmpty(processName))
                    continue;

                checkedCount++;
                var procLower = processName.ToLowerInvariant();
                if (IsLegitimateProcess(procLower))
                    continue;

                var executablePath = TryGetExecutablePath(process);
                var searchText = string.IsNullOrEmpty(executablePath)
                    ? procLower
                    : $"{procLower} {executablePath.ToLowerInvariant()}";

                foreach (var cheatSig in _cheatDb.CheatProcesses)
                {
                    if (!SignatureMatcher.MatchesText(searchText, cheatSig))
                        continue;

                    FoundCheats.Add(new DetectedThreat
                    {
                        Type = "process",
                        Name = processName,
                        Path = executablePath,
                        Signature = cheatSig,
                        Match = cheatSig,
                        Risk = "high"
                    });
                    break;
                }
            }
        }

        return checkedCount;
    }

    private static string TryGetExecutablePath(Process process)
    {
        try
        {
            return process.MainModule?.FileName ?? "";
        }
        catch
        {
            return "";
        }
    }

    private bool IsLegitimateProcess(string procName)
    {
        return Config.CheatSignatures.LegitimateProcesses.Contains(procName);
    }
}
