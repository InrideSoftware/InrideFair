using System.IO;
using System.Text.Json;

namespace InrideFair.Config;

public record ExternalSignatures
{
    public string Version { get; init; } = "1.0";
    public List<string> Processes { get; init; } = [];
    public List<string> Files { get; init; } = [];
    public List<string> ConfigFieldIndicators { get; init; } = [];
}

/// <summary>
/// Загрузка дополнительных сигнатур из signatures.json.
/// </summary>
public static class SignatureLoader
{
    private static readonly string SignaturesPath = Path.Combine(AppContext.BaseDirectory, "signatures.json");

    public static ExternalSignatures Load()
    {
        try
        {
            if (!File.Exists(SignaturesPath))
                return new ExternalSignatures();

            var json = File.ReadAllText(SignaturesPath);
            return JsonSerializer.Deserialize<ExternalSignatures>(json) ?? new ExternalSignatures();
        }
        catch
        {
            return new ExternalSignatures();
        }
    }
}
