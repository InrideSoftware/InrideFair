using System.IO;
using Serilog;
using Serilog.Events;

namespace InrideFair.Services;

/// <summary>
/// Сервис логирования.
/// </summary>
public interface ILoggingService
{
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Warning(string message, Exception? exception = null);
    void Error(string message, Exception? exception = null);
    void Fatal(string message, Exception? exception = null);
}

/// <summary>
/// Реализация сервиса логирования на основе Serilog.
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly Serilog.Core.Logger _logger;

    public LoggingService()
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "Logs", "inridefair-.log");
        
        // Создаём директорию для логов
        var logDirectory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                shared: true)
            .CreateLogger();
    }

    public void Debug(string message) => _logger.Write(LogEventLevel.Debug, message);
    public void Info(string message) => _logger.Write(LogEventLevel.Information, message);
    public void Warning(string message) => _logger.Write(LogEventLevel.Warning, message);
    public void Warning(string message, Exception? exception = null) => _logger.Write(LogEventLevel.Warning, exception, message);
    public void Error(string message, Exception? exception = null) => _logger.Write(LogEventLevel.Error, exception, message);
    public void Fatal(string message, Exception? exception = null) => _logger.Write(LogEventLevel.Fatal, exception, message);

    public void Dispose()
    {
        _logger.Dispose();
    }
}
