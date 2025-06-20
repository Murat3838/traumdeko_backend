using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Reflection;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using AutoMapper;
using VeragWebApp.Helper;
using VeragWebApp.Service;
using VeragWebApp.Repos;
using VeragWebApp.Container;
using VeragWebApp.Modal;
using System.Security.Claims;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;

ServicePointManager.SecurityProtocol =
      SecurityProtocolType.Tls    // TLS 1.0
    | SecurityProtocolType.Tls11  // TLS 1.1
    | SecurityProtocolType.Tls12  // TLS 1.2
    | SecurityProtocolType.Tls13; // TLS 1.3 

var builder = WebApplication.CreateBuilder(args);



builder.Services
    .AddHttpClient("LegacyTlsClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler
        {
            SslOptions = new SslClientAuthenticationOptions
            {
                // explizit auch TLS1.0 und TLS1.1 freigeben
                EnabledSslProtocols = SslProtocols.Tls    // TLS 1.0
                                    | SslProtocols.Tls11  // TLS 1.1
                                    | SslProtocols.Tls12  // TLS 1.2
                                    | SslProtocols.Tls13  // TLS 1.3
            }
        }
    );



// Registrierung der Konfigurationsklassen
 

// **WICHTIG:** Registrierung von IHttpClientFactory
builder.Services.AddHttpClient();

 



// Registrierung von benutzerbezogenen Services (User, Roles, Refresh Token Handler)
builder.Services.AddTransient<IRefreshHandler, RefreshHandler>();

// Konfiguration des Datenbankkontexts für Benutzer- und Rollendaten
builder.Services.AddDbContext<VeragDB>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36)) // Passe ggf. die Version an
    )
);

// JWT Settings Konfiguration – Annahme, dass in der Konfiguration unter "JwtSettings" entsprechende Werte hinterlegt sind
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var authKey = jwtSettingsSection.GetValue<string>("securitykey");

// Registrierung von cTimasAPI mit allen notwendigen Abhängigkeiten
 

// Einrichtung der JWT-Authentifizierung
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        RoleClaimType = ClaimTypes.Role,  
        ClockSkew = TimeSpan.Zero
    };
});

// Konfiguration von AutoMapper (sofern benötigt)
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMapperHandler());
});
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

// ------------------------------------------------------------------
// Ende der zusätzlichen Registrierung für Login/Authentifizierung
// ------------------------------------------------------------------

// **WICHTIG:** Hinzufügen von Controllers
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Hinzufügen von Swagger für die API-Dokumentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AvisoAPI",
        Version = "v1",
        Description = "API zur Verwaltung von Aviso-Daten."
    });

    // Einbinden der XML-Dokumentationsdatei
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Optional: Weitere Swagger-Konfigurationen (z.B. Sicherheit) hinzufügen
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.WithOrigins("https://zeit.app.verag.ag", "http://localhost:8100", "http://localhost:80") // Ersetze durch deine tatsächliche Origin
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()); // Erlaube Credentials wie Cookies
});

// **6. Logging mit Serilog**
// Hauptlogger für die Anwendung
var mainLogger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: "G:\\ZeiterfassungLogs\\log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
        .WriteTo.File(
            path: "G:\\ZeiterfassungLogs\\error-log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        )
    )
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(mainLogger);

var requestLogger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: "G:\\ZeiterfassungLogs\\request-log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Message:lj}{NewLine}"
    )
    .CreateLogger();
// Registrieren des separaten Request-Loggers im DI-Container
builder.Services.AddSingleton<Serilog.ILogger>(requestLogger);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Aktiviere Swagger und Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AvisoAPI v1"));
        c.RoutePrefix = string.Empty; // Swagger UI unter der Root-URL verfügbar machen
    });
}

// Optional: HTTPS-Weiterleitung aktivieren
// app.UseHttpsRedirection();

app.UseRouting();

// CORS Middleware mit der definierten Richtlinie
app.UseCors("CorsPolicy");

// ------------------------------------------------------------------
// Aktivierung der Authentifizierung und Autorisierung (relevant für Login)
// ------------------------------------------------------------------
app.UseAuthentication();
app.UseAuthorization();

// Zuweisung der Controller-Routen
app.MapControllers();

app.Run();
