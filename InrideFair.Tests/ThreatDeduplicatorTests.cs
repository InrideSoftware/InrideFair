using InrideFair.Models;
using InrideFair.Utils;

namespace InrideFair.Tests;

public class ThreatDeduplicatorTests
{
    [Fact]
    public void Deduplicate_RemovesDuplicateThreats()
    {
        var threats = new List<DetectedThreat>
        {
            new() { Type = "browser_search", Url = "https://example.test", Match = "cheat" },
            new() { Type = "browser_search", Url = "https://example.test", Match = "cheat" },
            new() { Type = "file", Path = @"C:\temp\injector.exe", Match = "injector.exe" }
        };

        var result = ThreatDeduplicator.Deduplicate(threats);

        Assert.Equal(2, result.Count);
    }
}
