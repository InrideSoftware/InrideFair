using InrideFair.Utils;

namespace InrideFair.Tests;

public class ReportFileDetectorTests
{
    [Theory]
    [InlineData(@"C:\Users\Simon\Downloads\inridefair_report.json", true)]
    [InlineData(@"C:\Users\Simon\Downloads\Telegram Desktop\inridefair_report (2).json", true)]
    [InlineData(@"C:\Apps\InrideFair\inridefair_report.html", true)]
    [InlineData(@"C:\Apps\cheat_config.json", false)]
    [InlineData(@"C:\Apps\inridefair_report.txt", false)]
    public void IsReportFilePath_DetectsOwnReports(string path, bool expected)
    {
        Assert.Equal(expected, ReportFileDetector.IsReportFilePath(path));
    }

    [Fact]
    public void IsReportJsonContent_DetectsSerializedScanReport()
    {
        var json = """
            {
              "Version": "1.1.0",
              "ScanDate": "2026-06-05T22:00:00",
              "SystemInfo": "Windows",
              "Summary": { "TotalThreats": 1, "HighRisk": 1, "MediumRisk": 0, "LowRisk": 0 },
              "Processes": [],
              "Files": [{ "Type": "file", "Match": "exloader", "Risk": "high" }],
              "Archives": [],
              "Browser": [],
              "Registry": []
            }
            """;

        Assert.True(ReportFileDetector.IsReportJsonContent(json));
    }

    [Fact]
    public void AnalyzeConfigFile_SkipsOwnReportJson()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inridefair-report-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var reportPath = Path.Combine(tempDir, "inridefair_report.json");
            File.WriteAllText(reportPath, """
                {
                  "Version": "1.1.0",
                  "ScanDate": "2026-06-05T22:00:00",
                  "SystemInfo": "Windows",
                  "Summary": { "TotalThreats": 1, "HighRisk": 1, "MediumRisk": 0, "LowRisk": 0 },
                  "Processes": [],
                  "Files": [{ "Match": "exloader", "Indicators": ["aimbot", "esp", "visuals"] }],
                  "Archives": [],
                  "Browser": [],
                  "Registry": []
                }
                """);

            var (score, indicators) = FileAnalyzer.AnalyzeConfigFile(reportPath);

            Assert.Equal(0, score);
            Assert.Empty(indicators);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
