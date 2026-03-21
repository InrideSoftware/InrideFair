using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using InrideFair.Config;
using InrideFair.Scanner;
using InrideFair.Services;
using InrideFair.Utils;

namespace InrideFair.UI;

/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow : System.Windows.Window, IDisposable
{
    private readonly ILoggingService _logger;
    private bool _isScanning;
    private Dictionary<string, object?> _currentResult = new();
    private ScannerService? _scannerService;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _disposed;

    public MainWindow(ILoggingService logger, ScannerService scannerService)
    {
        _logger = logger;
        _scannerService = scannerService;

        InitializeComponent();
        InitializeWindow();

        _logger.Debug("MainWindow создан");
    }

    private void InitializeWindow()
    {
        // Установка заголовка с версией
        Title = $"{AppVersion.Product} {AppVersion.Display}";
        VersionTextBlock.Text = AppVersion.Display;
        AboutDescriptionText.Text = $"{AppVersion.Display} — это современное программное обеспечение для обнаружения и анализа читов, вредоносного ПО и подозрительной активности в системе.";

        // Установка информации о системе
        SystemInfoText.Text = GetSystemInfo();

        // Проверка прав администратора
        CheckAdminRights();

        _currentResult = new Dictionary<string, object?>
        {
            ["processes"] = new List<Dictionary<string, object?>>(),
            ["files"] = new List<Dictionary<string, object?>>(),
            ["archives"] = new List<Dictionary<string, object?>>(),
            ["browser"] = new List<Dictionary<string, object?>>(),
            ["registry"] = new List<Dictionary<string, object?>>()
        };

        _logger.Debug("MainWindow инициализирован");
    }

    private string GetSystemInfo()
    {
        var osName = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => GetWindowsProductName(),
            PlatformID.Unix => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macOS" : "Linux",
            _ => "Unknown"
        };

        return $"ОС: {osName}  |  Архитектура: {RuntimeInformation.OSArchitecture}  |  .NET: {Environment.Version}";
    }

    private string GetWindowsProductName()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var productName = key?.GetValue("ProductName")?.ToString() ?? $"Windows {Environment.OSVersion.Version}";
            return productName;
        }
        catch (Exception ex)
        {
            _logger.Warning($"Не удалось получить версию Windows: {ex.Message}");
            return $"Windows {Environment.OSVersion.Version}";
        }
    }

    private void CheckAdminRights()
    {
        if (!ProcessUtils.IsAdmin())
        {
            AppendLog("⚠️  Внимание: Запуск от имени администратора рекомендуется!");
            StatusLabel.Text = "● Требуется запуск от администратора";
            StatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 158, 11));
            _logger.Warning("Приложение запущено без прав администратора");
        }
    }

    private void AppendLog(string message)
    {
        LogText.AppendText($"{message}\n");
        LogText.ScrollToEnd();
        _logger.Debug(message);
    }

    private void UpdateStatusBadge(string text, string colorHex)
    {
        if (StatusBadge.Child is TextBlock badgeText)
        {
            badgeText.Text = text;
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
            badgeText.Foreground = new System.Windows.Media.SolidColorBrush(color);
        }
    }

    private (int total, int highRisk, int mediumRisk, int lowRisk) UpdateDetails()
    {
        var processes = _currentResult.GetValueOrDefault("processes") as List<Dictionary<string, object?>> ?? [];
        var files = _currentResult.GetValueOrDefault("files") as List<Dictionary<string, object?>> ?? [];
        var archives = _currentResult.GetValueOrDefault("archives") as List<Dictionary<string, object?>> ?? [];
        var browser = _currentResult.GetValueOrDefault("browser") as List<Dictionary<string, object?>> ?? [];
        var registry = _currentResult.GetValueOrDefault("registry") as List<Dictionary<string, object?>> ?? [];

        var total = processes.Count + files.Count + archives.Count + browser.Count + registry.Count;
        var highRisk = processes.Count + files.Count;
        var mediumRisk = browser.Count + registry.Count + archives.Count;
        var lowRisk = 0;

        DetailsText.Text = $@"  • Процессы: {processes.Count}
  • Файлы: {files.Count}
  • Архивы: {archives.Count}
  • Браузеры: {browser.Count}
  • Реестр: {registry.Count}

  🔴 Высокий риск: {highRisk}
  🟡 Средний риск: {mediumRisk}
  ─────────────────
  ВСЕГО: {total} угроз";

        // Обновляем карточки
        TotalCount.Text = total.ToString();
        HighRiskCount.Text = highRisk.ToString();
        MediumRiskCount.Text = mediumRisk.ToString();
        LowRiskCount.Text = lowRisk.ToString();

        return (total, highRisk, mediumRisk, lowRisk);
    }

    private async void ScanButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isScanning)
        {
            _logger.Warning("Попытка повторного запуска сканирования");
            return;
        }

        _logger.Info("Запуск сканирования пользователем");

        _isScanning = true;
        ScanButton.IsEnabled = false;
        OpenReportButton.IsEnabled = false;
        OpenFolderButton.IsEnabled = false;

        // Очистка лога
        LogText.Clear();
        AppendLog("Запуск модуля проверки...");

        // Очистка деталей
        DetailsText.Text = "  • Запуск проверки...\n";

        // Сброс прогресса и карточек
        ProgressBar.Value = 0;
        ProgressLabel.Text = "Запуск сканера...";
        StatusLabel.Text = "● Проверка запущена";
        StatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 130, 246));
        StatusIndicator.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 130, 246));
        UpdateStatusBadge("СКАНИРОВАНИЕ", "#3B82F6");
        
        // Сброс карточек
        TotalCount.Text = "0";
        HighRiskCount.Text = "0";
        MediumRiskCount.Text = "0";
        LowRiskCount.Text = "0";

        // Запуск сканирования
        await RunScanAsync();
    }

    private async Task RunScanAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            // Создаём новый сервис для каждого сканирования
            _scannerService?.Dispose();
            _scannerService = ServiceContainer.GetRequiredService<ScannerService>();

            var progress = new Progress<string>(message => AppendLog(message));
            var results = await _scannerService.RunScanAsync(progress);

            if (results.Error != null)
            {
                _logger.Error("Ошибка в сканере", new Exception(results.Error));
                AppendLog($"❌ Ошибка в сканере: {results.Error}");
            }

            // Сохранение логов
            foreach (var logLine in results.Log)
            {
                AppendLog(logLine);
            }

            // Сохранение результатов
            _currentResult = new Dictionary<string, object?>
            {
                ["processes"] = results.Processes,
                ["files"] = results.Files,
                ["archives"] = results.Archives,
                ["browser"] = results.Browser,
                ["registry"] = results.Registry
            };

            ScanComplete(results);
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка при сканировании", ex);
            AppendLog($"❌ Ошибка: {ex.Message}");
            ScanButton.IsEnabled = true;
            _isScanning = false;
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void ScanComplete(ScanResult results)
    {
        _isScanning = false;
        ScanButton.IsEnabled = true;
        OpenReportButton.IsEnabled = true;
        OpenFolderButton.IsEnabled = true;
        ProgressBar.Value = 100;
        ProgressLabel.Text = "Проверка завершена";

        var (total, highRisk, mediumRisk, lowRisk) = UpdateDetails();

        if (total == 0)
        {
            StatusLabel.Text = "● Проверка завершена: чисто";
            StatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129));
            UpdateStatusBadge("ЧИСТО", "#10B981");
            StatusIndicator.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129));
            AppendLog("✅ Проверка завершена: угроз не обнаружено");
            _logger.Info("Проверка завершена: угроз не обнаружено");
        }
        else
        {
            var statusText = highRisk > 0 ? "ВЫСОКИЙ РИСК" : "СРЕДНИЙ РИСК";
            var colorHex = highRisk > 0 ? "#F472B6" : "#FBBF24";
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
            
            StatusLabel.Text = $"● Найдено угроз: {total}";
            StatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(color);
            UpdateStatusBadge(statusText, colorHex);
            StatusIndicator.Background = new System.Windows.Media.SolidColorBrush(color);
            
            AppendLog($"⚠️  ВСЕГО УГРОЗ: {total}");
            AppendLog($"   🔴 Высокий риск: {highRisk}");
            AppendLog($"   🟡 Средний риск: {mediumRisk}");
            _logger.Warning($"Проверка завершена: найдено {total} угроз");
        }

        // Сохранение отчёта
        SaveReport();
    }

    private async void SaveReport()
    {
        try
        {
            var processes = _currentResult.GetValueOrDefault("processes") as List<Dictionary<string, object?>> ?? [];
            var files = _currentResult.GetValueOrDefault("files") as List<Dictionary<string, object?>> ?? [];
            var archives = _currentResult.GetValueOrDefault("archives") as List<Dictionary<string, object?>> ?? [];
            var browser = _currentResult.GetValueOrDefault("browser") as List<Dictionary<string, object?>> ?? [];
            var registry = _currentResult.GetValueOrDefault("registry") as List<Dictionary<string, object?>> ?? [];

            var total = processes.Count + files.Count + archives.Count + browser.Count + registry.Count;
            var highRisk = processes.Count + files.Count;
            var mediumRisk = browser.Count + registry.Count + archives.Count;

            // Создаём данные отчёта в новом формате
            var reportData = new ScanResultData
            {
                Version = AppVersion.Full,
                ScanDate = DateTime.Now,
                SystemInfo = GetSystemInfo(),
                Summary = new SummaryData
                {
                    TotalThreats = total,
                    HighRisk = highRisk,
                    MediumRisk = mediumRisk,
                    LowRisk = 0
                },
                Processes = processes.Select(ToThreatData).ToList(),
                Files = files.Select(ToThreatData).ToList(),
                Archives = archives.Select(ToThreatData).ToList(),
                Browser = browser.Select(ToThreatData).ToList(),
                Registry = registry.Select(ToThreatData).ToList()
            };

            // Получаем сервис отчётов
            var reportService = ServiceContainer.GetRequiredService<IReportService>();
            
            // Сохраняем JSON отчёт
            var jsonReportPath = Path.Combine(AppContext.BaseDirectory, "inridefair_report.json");
            reportService.GenerateJsonReport(reportData, jsonReportPath);

            // Сохраняем HTML отчёт
            var htmlReportPath = Path.Combine(AppContext.BaseDirectory, "inridefair_report.html");
            await reportService.GenerateHtmlReportAsync(reportData, htmlReportPath);

            AppendLog($"📄 Отчёты сохранены");
            _logger.Info($"Отчёты сохранены: {jsonReportPath}, {htmlReportPath}");
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка при сохранении отчёта", ex);
            AppendLog($"❌ Ошибка при сохранении отчёта: {ex.Message}");
        }
    }

    private static ThreatData ToThreatData(Dictionary<string, object?> dict)
    {
        return new ThreatData
        {
            Type = dict.GetValueOrDefault("type")?.ToString() ?? "",
            Path = dict.GetValueOrDefault("path")?.ToString() ?? "",
            Match = dict.GetValueOrDefault("match")?.ToString() ?? "",
            Hash = dict.GetValueOrDefault("hash")?.ToString() ?? "",
            Risk = dict.GetValueOrDefault("risk")?.ToString() ?? "medium",
            AnalysisScore = dict.GetValueOrDefault("analysis_score") as int?,
            Indicators = dict.GetValueOrDefault("indicators") as List<string> ?? []
        };
    }

    private void OpenReportButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            var reportPath = Path.Combine(AppContext.BaseDirectory, "inridefair_report.html");
            if (File.Exists(reportPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = reportPath,
                    UseShellExecute = true
                });
                _logger.Debug($"Открыт HTML отчёт: {reportPath}");
            }
            else
            {
                AppendLog("❌ Отчёт не найден");
                _logger.Warning("HTML отчёт не найден при попытке открытия");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка при открытии отчёта", ex);
            AppendLog($"❌ Ошибка: {ex.Message}");
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Двойной клик - разворачиваем/восстанавливаем окно
            this.WindowState = this.WindowState == System.Windows.WindowState.Maximized
                ? System.Windows.WindowState.Normal
                : System.Windows.WindowState.Maximized;
        }
        else
        {
            // Одинарный клик - перетаскиваем окно
            this.DragMove();
        }
    }

    private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void MinimizeButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        this.WindowState = System.Windows.WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        this.WindowState = this.WindowState == System.Windows.WindowState.Maximized
            ? System.Windows.WindowState.Normal
            : System.Windows.WindowState.Maximized;
    }

    private void MenuButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not Button button) return;

        // Сброс всех кнопок
        HomeButton.Tag = null;
        AboutButton.Tag = null;
        QnAButton.Tag = null;
        DeveloperButton.Tag = null;
        GitHubButton.Tag = null;

        // Переключение видов
        HomeView.Visibility = System.Windows.Visibility.Collapsed;
        AboutView.Visibility = System.Windows.Visibility.Collapsed;
        QnAView.Visibility = System.Windows.Visibility.Collapsed;
        DeveloperView.Visibility = System.Windows.Visibility.Collapsed;
        GitHubView.Visibility = System.Windows.Visibility.Collapsed;

        switch (button.Name)
        {
            case "HomeButton":
                HomeView.Visibility = System.Windows.Visibility.Visible;
                button.Tag = "True";
                break;
            case "AboutButton":
                AboutView.Visibility = System.Windows.Visibility.Visible;
                button.Tag = "True";
                break;
            case "QnAButton":
                QnAView.Visibility = System.Windows.Visibility.Visible;
                button.Tag = "True";
                break;
            case "DeveloperButton":
                DeveloperView.Visibility = System.Windows.Visibility.Visible;
                button.Tag = "True";
                break;
            case "GitHubButton":
                GitHubView.Visibility = System.Windows.Visibility.Visible;
                button.Tag = "True";
                break;
        }
    }

    private void OpenGitHubButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/InrideSoftware",
                UseShellExecute = true
            });
            _logger.Debug("Открыт GitHub");
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка при открытии GitHub", ex);
        }
    }

    private void OpenFolderButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            var files = _currentResult.GetValueOrDefault("files") as List<Dictionary<string, object?>> ?? [];

            if (files.Count == 0)
            {
                AppendLog("ℹ️  Нет папок для открытия");
                return;
            }

            var opened = new HashSet<string>();
            foreach (var threat in files)
            {
                if (threat.TryGetValue("path", out var pathObj) && pathObj is string filepath && !string.IsNullOrEmpty(filepath))
                {
                    if (!opened.Contains(filepath))
                    {
                        try
                        {
                            ProcessUtils.OpenFolder(filepath);
                            opened.Add(filepath);
                            AppendLog($"📂 Открыто: {Path.GetDirectoryName(filepath)}");
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning($"Не удалось открыть папку: {filepath}. {ex.Message}");
                        }
                    }
                }
            }

            if (opened.Count > 0)
                AppendLog($"✅ Открыто папок: {opened.Count}");
            else
                AppendLog("ℹ️  Папки не найдены");
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка при открытии папок", ex);
            AppendLog($"❌ Ошибка: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _logger.Info("MainWindow закрыт");
        Dispose();
        base.OnClosed(e);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.Debug("MainWindow disposed");
        _disposed = true;
        _scannerService?.Dispose();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }

    ~MainWindow()
    {
        Dispose();
    }
}
