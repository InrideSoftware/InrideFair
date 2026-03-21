using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using InrideFair.Config;
using InrideFair.Scanner;
using InrideFair.Services;
using InrideFair.UI;

namespace InrideFair;

public partial class App : Application
{
    private ILoggingService _logger = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Инициализация логирования
        _logger = ServiceContainer.GetRequiredService<ILoggingService>();
        _logger.Info($"=== Запуск приложения {AppVersion.Product} {AppVersion.Display} ===");

        // Обработка необработанных исключений
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        try
        {
            // Установить рабочую директорию
            var appPath = AppContext.BaseDirectory;
            Directory.SetCurrentDirectory(appPath);
            _logger.Debug($"Рабочая директория: {appPath}");

            // Проверка аргумента --scan
            var args = e.Args;
            if (args.Length > 0 && args[0] == "--scan")
            {
                _logger.Info("Запуск в режиме сканирования (console)");
                await RunConsoleScanAsync();
                Shutdown();
                return;
            }

            // Запуск GUI
            _logger.Info("Запуск GUI режима");
            var mainWindow = ServiceContainer.GetRequiredService<MainWindow>();
            mainWindow.Show();
            _logger.Debug("MainWindow отображён");
        }
        catch (Exception ex)
        {
            _logger.Fatal("Критическая ошибка при запуске", ex);
            MessageBox.Show($"Критическая ошибка при запуске: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        _logger.Error("Необработанное исключение WPF", e.Exception);
        MessageBox.Show($"Произошла ошибка: {e.Exception.Message}", "Ошибка",
            MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        _logger.Fatal("Необработанное исключение домена", exception);
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.Error("Необработанное исключение задачи", e.Exception);
        e.SetObserved();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger.Info("=== Завершение приложения Inride Fair ===");
        base.OnExit(e);
    }

    private async Task RunConsoleScanAsync()
    {
        try
        {
            var scannerService = ServiceContainer.GetRequiredService<ScannerService>();
            var results = await scannerService.RunScanAsync(null);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Default
            };
            var json = JsonSerializer.Serialize(results, options);
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(json);

            _logger.Info("Сканирование завершено");
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка при сканировании", ex);
            throw;
        }
    }
}
