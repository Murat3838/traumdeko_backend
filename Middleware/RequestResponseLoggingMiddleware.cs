using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public class SimpleRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Serilog.ILogger _logger;

    public SimpleRequestLoggingMiddleware(RequestDelegate next, Serilog.ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // Informationen aus der aktuellen Anfrage auslesen
        string userAgent = context.Request.Headers["User-Agent"].ToString();
        string device = string.IsNullOrEmpty(userAgent)
            ? "unknown"
            : userAgent.Substring(0, Math.Min(userAgent.Length, 50)); // Kürze den User-Agent auf 50 Zeichen

        string apiRoute = $"{context.Request.Path}{context.Request.QueryString}";
        string remoteIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string method = context.Request.Method;
        // Formatierter Zeitstempel, z. B. "2025-01-14 15:30:45"
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        // Erstelle ein Objekt, das alle relevanten Informationen enthält
        var requestInfo = new
        {
            Route = apiRoute,
            Method = method,
            Device = device,
            Origin = remoteIpAddress,
            Timestamp = timestamp
        };

        // Serialisiere das Objekt als indented (formatierter) JSON-String
        string prettyJson = JsonSerializer.Serialize(requestInfo,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        // Logge den formatierten JSON-String
        _logger.Information("API Request:{NewLine}{RequestInfo}", Environment.NewLine, prettyJson);

        // Nächste Middleware in der Pipeline aufrufen
        await _next(context);
    }
}
