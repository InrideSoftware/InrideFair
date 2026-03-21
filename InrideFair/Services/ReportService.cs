using System.IO;
using System.Text;
using System.Text.Json;
using InrideFair.Config;

namespace InrideFair.Services;

/// <summary>
/// Сервис генерации отчётов.
/// </summary>
public interface IReportService
{
    Task<string> GenerateHtmlReportAsync(ScanResultData data, string outputPath);
    string GenerateJsonReport(ScanResultData data, string outputPath);
}

/// <summary>
/// Данные отчёта о сканировании.
/// </summary>
public record ScanResultData
{
    public string Version { get; init; } = AppVersion.Full;
    public DateTime ScanDate { get; init; }
    public string SystemInfo { get; init; } = "";
    public SummaryData Summary { get; init; } = new();
    public List<ThreatData> Processes { get; init; } = [];
    public List<ThreatData> Files { get; init; } = [];
    public List<ThreatData> Archives { get; init; } = [];
    public List<ThreatData> Browser { get; init; } = [];
    public List<ThreatData> Registry { get; init; } = [];
}

/// <summary>
/// Сводные данные отчёта.
/// </summary>
public record SummaryData
{
    public int TotalThreats { get; init; }
    public int HighRisk { get; init; }
    public int MediumRisk { get; init; }
    public int LowRisk { get; init; }
}

/// <summary>
/// Данные об угрозе.
/// </summary>
public record ThreatData
{
    public string Type { get; init; } = "";
    public string Path { get; init; } = "";
    public string Match { get; init; } = "";
    public string Hash { get; init; } = "";
    public string Risk { get; init; } = "";
    public int? AnalysisScore { get; init; }
    public List<string> Indicators { get; init; } = [];
}

/// <summary>
/// Реализация сервиса отчётов.
/// </summary>
public class ReportService : IReportService
{
    private readonly ILoggingService _logger;

    public ReportService(ILoggingService logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateHtmlReportAsync(ScanResultData data, string outputPath)
    {
        var html = GenerateHtmlContent(data);
        await File.WriteAllTextAsync(outputPath, html, Encoding.UTF8);
        _logger.Info($"HTML отчёт сохранён: {outputPath}");
        return outputPath;
    }

    public string GenerateJsonReport(ScanResultData data, string outputPath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default
        };
        var json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(outputPath, json, Encoding.UTF8);
        _logger.Info($"JSON отчёт сохранён: {outputPath}");
        return outputPath;
    }

    private string GenerateHtmlContent(ScanResultData data)
    {
        var riskColor = data.Summary.TotalThreats == 0 ? "#10b981" : 
                        data.Summary.HighRisk > 0 ? "#ef4444" : "#f59e0b";
        var riskText = data.Summary.TotalThreats == 0 ? "ЧИСТО" : 
                       data.Summary.HighRisk > 0 ? "ВЫСОКИЙ РИСК" : "СРЕДНИЙ РИСК";
        var riskBg = data.Summary.TotalThreats == 0 ? "rgb(16, 185, 129, 0.1)" : 
                     data.Summary.HighRisk > 0 ? "rgb(239, 68, 68, 0.1)" : "rgb(245, 158, 11, 0.1)";

        return $@"<!DOCTYPE html>
<html lang=""ru"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Inride Fair — Отчёт о сканировании</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
            color: #e4e4e4;
            min-height: 100vh;
            padding: 40px 20px;
            line-height: 1.6;
        }}
        .container {{ max-width: 1200px; margin: 0 auto; }}
        
        /* Header */
        .header {{
            text-align: center;
            margin-bottom: 40px;
            padding: 30px;
            background: rgba(255,255,255,0.03);
            border-radius: 16px;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.1);
        }}
        .header h1 {{
            font-size: 2.5rem;
            font-weight: 700;
            margin-bottom: 10px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }}
        .header .subtitle {{ color: #94a3b8; font-size: 0.95rem; }}
        .header .scan-date {{ color: #64748b; font-size: 0.85rem; margin-top: 8px; }}
        
        /* Status Card */
        .status-card {{
            background: {riskBg};
            border: 2px solid {riskColor};
            border-radius: 16px;
            padding: 30px;
            text-align: center;
            margin-bottom: 30px;
        }}
        .status-card .status-icon {{
            font-size: 3rem;
            margin-bottom: 15px;
        }}
        .status-card .status-text {{
            font-size: 1.8rem;
            font-weight: 700;
            color: {riskColor};
            letter-spacing: 2px;
        }}
        .status-card .status-subtext {{
            color: #94a3b8;
            margin-top: 10px;
        }}
        
        /* Stats Grid */
        .stats-grid {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }}
        .stat-card {{
            background: rgba(255,255,255,0.03);
            border-radius: 12px;
            padding: 25px;
            text-align: center;
            border: 1px solid rgba(255,255,255,0.08);
            transition: transform 0.2s, box-shadow 0.2s;
        }}
        .stat-card:hover {{
            transform: translateY(-2px);
            box-shadow: 0 10px 40px rgba(0,0,0,0.3);
        }}
        .stat-card .stat-value {{
            font-size: 2.5rem;
            font-weight: 700;
            margin-bottom: 5px;
        }}
        .stat-card .stat-label {{
            color: #94a3b8;
            font-size: 0.9rem;
            text-transform: uppercase;
            letter-spacing: 1px;
        }}
        .stat-card.total .stat-value {{ color: #667eea; }}
        .stat-card.high .stat-value {{ color: #ef4444; }}
        .stat-card.medium .stat-value {{ color: #f59e0b; }}
        .stat-card.low .stat-value {{ color: #10b981; }}
        
        /* Info Section */
        .info-section {{
            background: rgba(255,255,255,0.03);
            border-radius: 12px;
            padding: 20px 25px;
            margin-bottom: 30px;
            border: 1px solid rgba(255,255,255,0.08);
        }}
        .info-section h3 {{
            color: #94a3b8;
            font-size: 0.85rem;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 10px;
        }}
        .info-section p {{ color: #e4e4e4; font-size: 0.95rem; }}
        
        /* Threats Section */
        .threats-section {{
            background: rgba(255,255,255,0.03);
            border-radius: 16px;
            padding: 30px;
            border: 1px solid rgba(255,255,255,0.08);
        }}
        .threats-section h2 {{
            font-size: 1.5rem;
            margin-bottom: 25px;
            color: #f1f5f9;
        }}
        .threat-category {{
            margin-bottom: 30px;
        }}
        .threat-category:last-child {{ margin-bottom: 0; }}
        .threat-category h3 {{
            font-size: 1.1rem;
            color: #94a3b8;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 1px solid rgba(255,255,255,0.1);
        }}
        
        /* Threat Item */
        .threat-item {{
            background: rgba(255,255,255,0.02);
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 15px;
            border-left: 4px solid;
            transition: background 0.2s;
        }}
        .threat-item:hover {{
            background: rgba(255,255,255,0.04);
        }}
        .threat-item.high {{ border-left-color: #ef4444; }}
        .threat-item.medium {{ border-left-color: #f59e0b; }}
        .threat-item.low {{ border-left-color: #10b981; }}
        
        .threat-item .threat-header {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 12px;
            flex-wrap: wrap;
            gap: 10px;
        }}
        .threat-item .threat-path {{
            font-family: 'Consolas', 'Monaco', monospace;
            font-size: 0.9rem;
            color: #e4e4e4;
            word-break: break-all;
        }}
        .threat-item .threat-risk {{
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 0.75rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }}
        .threat-item .threat-risk.high {{
            background: rgba(239, 68, 68, 0.2);
            color: #ef4444;
        }}
        .threat-item .threat-risk.medium {{
            background: rgba(245, 158, 11, 0.2);
            color: #f59e0b;
        }}
        .threat-item .threat-risk.low {{
            background: rgba(16, 185, 129, 0.2);
            color: #10b981;
        }}
        
        .threat-item .threat-details {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
            gap: 12px;
            margin-top: 15px;
            padding-top: 15px;
            border-top: 1px solid rgba(255,255,255,0.05);
        }}
        .threat-detail {{
            font-size: 0.85rem;
        }}
        .threat-detail .label {{
            color: #64748b;
            display: block;
            margin-bottom: 4px;
        }}
        .threat-detail .value {{
            color: #e4e4e4;
            font-family: 'Consolas', 'Monaco', monospace;
            word-break: break-all;
        }}
        
        .threat-indicators {{
            margin-top: 12px;
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
        }}
        .indicator-tag {{
            background: rgba(102, 126, 234, 0.15);
            color: #667eea;
            padding: 4px 10px;
            border-radius: 6px;
            font-size: 0.8rem;
            border: 1px solid rgba(102, 126, 234, 0.3);
        }}
        
        /* Empty State */
        .empty-state {{
            text-align: center;
            padding: 60px 20px;
            color: #64748b;
        }}
        .empty-state .icon {{
            font-size: 4rem;
            margin-bottom: 20px;
            opacity: 0.5;
        }}
        
        /* Footer */
        .footer {{
            text-align: center;
            margin-top: 40px;
            padding-top: 30px;
            border-top: 1px solid rgba(255,255,255,0.1);
            color: #64748b;
            font-size: 0.85rem;
        }}
        
        /* Responsive */
        @media (max-width: 768px) {{
            body {{ padding: 20px 10px; }}
            .header h1 {{ font-size: 1.8rem; }}
            .stats-grid {{ grid-template-columns: 1fr 1fr; }}
            .threat-item .threat-header {{ flex-direction: column; align-items: flex-start; }}
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <!-- Header -->
        <div class=""header"">
            <h1>🛡️ Inride Fair</h1>
            <p class=""subtitle"">Отчёт о сканировании системы</p>
            <p class=""scan-date"">{data.ScanDate:dd MMMM yyyy HH:mm:ss}</p>
        </div>
        
        <!-- Status Card -->
        <div class=""status-card"">
            <div class=""status-icon"">{(data.Summary.TotalThreats == 0 ? "✅" : data.Summary.HighRisk > 0 ? "⚠️" : "⚡")}</div>
            <div class=""status-text"">{riskText}</div>
            <div class=""status-subtext"">Обнаружено угроз: {data.Summary.TotalThreats}</div>
        </div>
        
        <!-- Stats Grid -->
        <div class=""stats-grid"">
            <div class=""stat-card total"">
                <div class=""stat-value"">{data.Summary.TotalThreats}</div>
                <div class=""stat-label"">Всего угроз</div>
            </div>
            <div class=""stat-card high"">
                <div class=""stat-value"">{data.Summary.HighRisk}</div>
                <div class=""stat-label"">Высокий риск</div>
            </div>
            <div class=""stat-card medium"">
                <div class=""stat-value"">{data.Summary.MediumRisk}</div>
                <div class=""stat-label"">Средний риск</div>
            </div>
            <div class=""stat-card low"">
                <div class=""stat-value"">{data.Summary.LowRisk}</div>
                <div class=""stat-label"">Низкий риск</div>
            </div>
        </div>
        
        <!-- System Info -->
        <div class=""info-section"">
            <h3>📊 Информация о системе</h3>
            <p>{data.SystemInfo}</p>
        </div>
        
        <!-- Threats Section -->
        {(data.Summary.TotalThreats > 0 ? $@"
        <div class=""threats-section"">
            <h2>📋 Обнаруженные угрозы</h2>
            
            {(data.Files.Count > 0 ? $@"
            <div class=""threat-category"">
                <h3>📁 Файлы ({data.Files.Count})</h3>
                {string.Join("", GenerateThreatItems(data.Files))}
            </div>
            " : "")}
            
            {(data.Processes.Count > 0 ? $@"
            <div class=""threat-category"">
                <h3>⚙️ Процессы ({data.Processes.Count})</h3>
                {string.Join("", GenerateThreatItems(data.Processes))}
            </div>
            " : "")}
            
            {(data.Archives.Count > 0 ? $@"
            <div class=""threat-category"">
                <h3>📦 Архивы ({data.Archives.Count})</h3>
                {string.Join("", GenerateThreatItems(data.Archives))}
            </div>
            " : "")}
            
            {(data.Browser.Count > 0 ? $@"
            <div class=""threat-category"">
                <h3>🌐 Браузер ({data.Browser.Count})</h3>
                {string.Join("", GenerateThreatItems(data.Browser))}
            </div>
            " : "")}
            
            {(data.Registry.Count > 0 ? $@"
            <div class=""threat-category"">
                <h3>🗂️ Реестр ({data.Registry.Count})</h3>
                {string.Join("", GenerateThreatItems(data.Registry))}
            </div>
            " : "")}
        </div>
        " : @"
        <div class=""threats-section"">
            <div class=""empty-state"">
                <div class=""icon"">🎉</div>
                <h2>Угроз не обнаружено!</h2>
                <p>Ваша система чиста. Продолжайте соблюдать меры безопасности.</p>
            </div>
        </div>
        ")}
        
        <!-- Footer -->
        <div class=""footer"">
            <p>Inride Fair v{data.Version} | © 2026 Inride Software. Все права защищены.</p>
            <p style=""margin-top: 8px;"">Отчёт сгенерирован автоматически</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateThreatItems(List<ThreatData> threats)
    {
        var sb = new StringBuilder();
        foreach (var threat in threats)
        {
            var riskClass = threat.Risk.ToLower();
            var riskText = threat.Risk switch
            {
                "high" => "Высокий",
                "medium" => "Средний",
                "low" => "Низкий",
                _ => threat.Risk
            };

            var indicatorsHtml = threat.Indicators.Count > 0
                ? "<div class=\"threat-indicators\">" +
                    string.Join("", threat.Indicators.Select(i => $"<span class=\"indicator-tag\">{System.Net.WebUtility.HtmlEncode(i)}</span>")) +
                   "</div>"
                : "";

            var scoreHtml = threat.AnalysisScore.HasValue
                ? $@"<div class=""threat-detail"">
                    <span class=""label"">Скор анализа</span>
                    <span class=""value"">{threat.AnalysisScore}/10</span>
                   </div>"
                : "";

            sb.Append($@"
            <div class=""threat-item {riskClass}"">
                <div class=""threat-header"">
                    <span class=""threat-path"">{System.Net.WebUtility.HtmlEncode(threat.Path)}</span>
                    <span class=""threat-risk {riskClass}"">{riskText}</span>
                </div>
                <div style=""color: #94a3b8; font-size: 0.9rem; margin-bottom: 10px;"">
                    <span style=""color: #64748b;"">Совпадение:</span> {System.Net.WebUtility.HtmlEncode(threat.Match)}
                </div>
                <div class=""threat-details"">
                    <div class=""threat-detail"">
                        <span class=""label"">Хеш (MD5)</span>
                        <span class=""value"">{threat.Hash}</span>
                    </div>
                    {scoreHtml}
                </div>
                {indicatorsHtml}
            </div>");
        }
        return sb.ToString();
    }
}
