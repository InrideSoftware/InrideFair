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
    public int CheckProcesses()
    {
        var processes = ProcessUtils.GetRunningProcesses();
        
        foreach (var proc in processes)
        {
            var procLower = proc.ToLower();
            foreach (var cheatSig in _cheatDb.CheatProcesses)
            {
                if (procLower.Contains(cheatSig.ToLower()) && !IsLegitimateProcess(procLower))
                {
                    FoundCheats.Add(new DetectedThreat
                    {
                        Type = "process",
                        Name = proc,
                        Signature = cheatSig,
                        Match = cheatSig,
                        Risk = "high"
                    });
                    break;
                }
            }
        }

        return processes.Count;
    }

    private bool IsLegitimateProcess(string procName)
    {
        return Config.CheatSignatures.LegitimateProcesses.Contains(procName);
    }
}
