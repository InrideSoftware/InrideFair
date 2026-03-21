using System.IO;
using System.Text.Json;
using InrideFair.Config;

namespace InrideFair.Services;

/// <summary>
/// Сервис валидации и загрузки конфигурации.
/// </summary>
public static class ConfigValidator
{
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.json");

    /// <summary>
    /// Проверить и загрузить конфигурацию.
    /// </summary>
    public static AppConfig ValidateAndLoad()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                CreateDefaultConfig();
                return new AppConfig();
            }

            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json);

            return config ?? new AppConfig();
        }
        catch (JsonException ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning($"Ошибка парсинга config.json: {ex.Message}. Используется конфигурация по умолчанию.");
            CreateDefaultConfig();
            return new AppConfig();
        }
        catch (Exception ex)
        {
            var logger = ServiceContainer.GetService<ILoggingService>();
            logger?.Warning($"Ошибка загрузки config.json: {ex.Message}. Используется конфигурация по умолчанию.");
            return new AppConfig();
        }
    }

    /// <summary>
    /// Создать конфигурацию по умолчанию.
    /// </summary>
    private static void CreateDefaultConfig()
    {
        var defaultConfig = new AppConfig();
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default
        };
        var json = JsonSerializer.Serialize(defaultConfig, options);
        File.WriteAllText(ConfigPath, json);
    }

    /// <summary>
    /// Сохранить конфигурацию.
    /// </summary>
    public static void Save(AppConfig config)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default
        };
        var json = JsonSerializer.Serialize(config, options);
        File.WriteAllText(ConfigPath, json);
    }
}
