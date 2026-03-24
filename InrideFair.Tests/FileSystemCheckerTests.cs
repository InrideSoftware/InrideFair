using InrideFair.Checkers;
using InrideFair.Config;
using InrideFair.Database;
using InrideFair.Services;

namespace InrideFair.Tests;

public class FileSystemCheckerTests
{
    [Theory]
    [InlineData(0, "Чисто")]
    [InlineData(1, "Низкая")]
    [InlineData(2, "Средняя")]
    [InlineData(3, "ВЫСОКАЯ")]
    [InlineData(9, "ВЫСОКАЯ")]
    public void GetSuspicionLabel_ReturnsExpectedLabel(int score, string expected)
    {
        var checker = CreateChecker();

        var label = checker.GetSuspicionLabel(score);

        Assert.Equal(expected, label);
    }

    [Fact]
    public void CheckDirectory_FindsKnownCheatFile_ByExactName()
    {
        var checker = CreateChecker();
        var tempDir = CreateTempDirectory();

        try
        {
            var suspiciousFile = Path.Combine(tempDir, "injector.exe");
            File.WriteAllText(suspiciousFile, "dummy");

            var result = checker.CheckDirectory(tempDir);

            Assert.Contains(result, threat =>
                threat.Type == "file" &&
                threat.Path == suspiciousFile &&
                threat.Match == "injector.exe" &&
                threat.ExactMatch);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CheckDirectory_FindsSuspiciousConfig_ByHeuristicAnalysis()
    {
        var checker = CreateChecker();
        var tempDir = CreateTempDirectory();

        try
        {
            var configPath = Path.Combine(tempDir, "settings_profile.json");
            var content = "{\"offsets\":{},\"aimbot\":true,\"visuals\":true}";
            File.WriteAllText(configPath, content);

            var result = checker.CheckDirectory(tempDir);

            Assert.Contains(result, threat =>
                threat.Type == "file" &&
                threat.Path == configPath &&
                threat.AnalysisScore is >= 5 &&
                threat.Match.StartsWith("Config:", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static FileSystemChecker CreateChecker()
    {
        var appConfig = new AppConfig
        {
            CustomExclusions = [],
            CustomSignatures = []
        };
        var db = new CheatDatabase(appConfig);
        return new FileSystemChecker(db, new NoOpLoggingService());
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"inridefair-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class NoOpLoggingService : ILoggingService
    {
        public void Debug(string message) { }
        public void Info(string message) { }
        public void Warning(string message) { }
        public void Warning(string message, Exception? exception = null) { }
        public void Error(string message, Exception? exception = null) { }
        public void Fatal(string message, Exception? exception = null) { }
    }
}
