using System.Reflection;

namespace InrideFair.Config;

/// <summary>
/// Информация о версии приложения.
/// </summary>
public static class AppVersion
{
    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    private static readonly Version? Version = Assembly.GetName().Version;

    /// <summary>
    /// Полная версия приложения (например, "1.1.0").
    /// </summary>
    public static string Full => Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.1.0";

    /// <summary>
    /// Короткая версия (например, "1.1").
    /// </summary>
    public static string Short => Version != null ? $"{Version.Major}.{Version.Minor}" : "1.1";

    /// <summary>
    /// Версия для отображения (например, "v1.1.0").
    /// </summary>
    public static string Display => $"v{Full}";

    /// <summary>
    /// Название продукта.
    /// </summary>
    public static string Product => Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Inride Fair";

    /// <summary>
    /// Авторы.
    /// </summary>
    public static string Authors => Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Inride Software";

    /// <summary>
    /// Копирайт.
    /// </summary>
    public static string Copyright => Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "© 2026 Inride Software";
}
