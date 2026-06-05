using InrideFair.Models;
using InrideFair.Scanner;
using InrideFair.Services;

namespace InrideFair.Tests;

public class ThreatMapperTests
{
    [Fact]
    public void CountByRisk_UsesThreatRiskField()
    {
        var result = new ScanResult
        {
            Processes =
            [
                new DetectedThreat { Risk = "high" }
            ],
            Browser =
            [
                new DetectedThreat { Risk = "low" },
                new DetectedThreat { Risk = "medium" }
            ]
        };

        var (total, high, medium, low) = ThreatMapper.CountByRisk(result.AllThreats());

        Assert.Equal(3, total);
        Assert.Equal(1, high);
        Assert.Equal(1, medium);
        Assert.Equal(1, low);
    }

    [Fact]
    public void ToThreatData_PreservesBrowserAndRegistryFields()
    {
        var threat = new DetectedThreat
        {
            Type = "browser_search",
            Browser = "Chrome (Profile 1)",
            Url = "https://example.test/cheat",
            Title = "Cheat download",
            Match = "neverlose",
            Risk = "medium"
        };

        var data = ThreatMapper.ToThreatData(threat);

        Assert.Equal("Chrome (Profile 1)", data.Browser);
        Assert.Equal("https://example.test/cheat", data.Url);
        Assert.Equal("Cheat download", data.Title);
        Assert.Equal("https://example.test/cheat", data.Path);
    }
}
