using InrideFair.Checkers;
using InrideFair.Config;
using InrideFair.Database;
using InrideFair.Services;

namespace InrideFair.Tests;

public class IntegrationScanTests
{
    [Fact]
    public void FileSystemChecker_FindsInjector_InTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inridefair-int-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var target = Path.Combine(tempDir, "injector.exe");
            File.WriteAllText(target, "test");

            var checker = CreateChecker();
            var found = checker.CheckDirectory(tempDir);

            Assert.Contains(found, t => t.Path == target);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CheatDatabase_ReloadSettings_PicksUpCustomSignature()
    {
        var db = new CheatDatabase(new AppConfig
        {
            CustomExclusions = [],
            CustomSignatures = ["my-custom-cheat"]
        });

        Assert.Contains(db.CheatProcesses, s => s == "my-custom-cheat");

        db.ReloadSettings(new AppConfig
        {
            CustomExclusions = [],
            CustomSignatures = ["another-signature"]
        });

        Assert.Contains(db.CheatProcesses, s => s == "another-signature");
        Assert.DoesNotContain(db.CheatProcesses, s => s == "my-custom-cheat");
    }

    private static FileSystemChecker CreateChecker()
    {
        var db = new CheatDatabase(new AppConfig { CustomExclusions = [], CustomSignatures = [] });
        return new FileSystemChecker(db, new NoOpLoggingService());
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
