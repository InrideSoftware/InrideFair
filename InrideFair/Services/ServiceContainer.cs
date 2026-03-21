using Microsoft.Extensions.DependencyInjection;
using InrideFair.Scanner;
using InrideFair.Checkers;
using InrideFair.Database;
using InrideFair.Services;
using InrideFair.UI;

namespace InrideFair.Services;

/// <summary>
/// Контейнер зависимостей приложения.
/// </summary>
public static class ServiceContainer
{
    private static IServiceProvider? _instance;

    public static IServiceProvider Instance => _instance ??= BuildServiceProvider();

    public static T GetRequiredService<T>() where T : notnull
    {
        return Instance.GetRequiredService<T>();
    }

    public static T? GetService<T>() where T : notnull
    {
        return Instance.GetService<T>();
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Логирование
        services.AddSingleton<ILoggingService, LoggingService>();

        // База данных
        services.AddSingleton<CheatDatabase>();

        // Чекеры
        services.AddSingleton<ProcessChecker>();
        services.AddSingleton<FileSystemChecker>();
        services.AddSingleton<ArchiveChecker>();
        services.AddSingleton<BrowserChecker>();
        services.AddSingleton<RegistryChecker>();

        // Сканер
        services.AddSingleton<ScannerService>();

        // Сервис отчётов
        services.AddSingleton<IReportService, ReportService>();

        // UI
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}
