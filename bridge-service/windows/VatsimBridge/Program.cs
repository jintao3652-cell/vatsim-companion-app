using VatsimBridge.Hubs;
using VatsimBridge.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/bridge-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    Log.Information("Starting VATSIM Companion Bridge");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Validate critical configuration at startup
    var jwtSecret = builder.Configuration["Jwt:SecretKey"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
    {
        Log.Fatal("Jwt:SecretKey must be at least 32 characters long!");
        return 1;
    }

    if (string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
    {
        Log.Warning("Jwt:Issuer or Jwt:Audience not configured!");
    }

// Listen on all network interfaces so phones on the same LAN can connect
var bridgePort = builder.Configuration["Port"] ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{bridgePort}");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR(options =>
{
    // Long Polling 经高延迟隧道，放宽心跳超时避免误判掉线
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(120);
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
})
.AddJsonProtocol(options =>
{
    // App 端按 camelCase 读取(from/content/messageType)，统一序列化命名
    options.PayloadSerializerOptions.PropertyNamingPolicy =
        System.Text.Json.JsonNamingPolicy.CamelCase;
});

// CORS - Allow SignalR connections
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
if (!string.IsNullOrEmpty(jwtSecretKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Allow JWT in SignalR connections
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/vatsimhub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
}

// HTTP Clients
builder.Services.AddHttpClient<IPluginCommunicationService, PluginCommunicationService>();
builder.Services.AddHttpClient<IPushNotificationService, PushNotificationService>();
builder.Services.AddHttpClient(); // 供 VatsimDataService 用的命名工厂

// Services
builder.Services.AddSingleton<IPluginCommunicationService, PluginCommunicationService>();
builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();
builder.Services.AddSingleton<IPairingService, PairingService>();
builder.Services.AddSingleton<IMessageStorageService, MessageStorageService>();

// VATSIM datafeed: 同一实例既作单例供查询，又作后台服务拉取
builder.Services.AddSingleton<VatsimBridge.Services.VatsimDataService>();
builder.Services.AddSingleton<VatsimBridge.Services.IVatsimDataService>(
    sp => sp.GetRequiredService<VatsimBridge.Services.VatsimDataService>());
builder.Services.AddHostedService(
    sp => sp.GetRequiredService<VatsimBridge.Services.VatsimDataService>());

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<VatsimHub>("/vatsimhub");

// Welcome page
app.MapGet("/", () => Results.Json(new
{
    name = "VATSIM Companion Bridge",
    version = "1.0.0",
    status = "running",
    endpoints = new
    {
        signalr = "/vatsimhub",
        api = "/api",
        swagger = "/swagger"
    }
}));

Console.WriteLine("===========================================");
Console.WriteLine("VATSIM Companion Bridge");
Console.WriteLine("===========================================");
Console.WriteLine($"Bridge URL: http://localhost:{builder.Configuration["Port"] ?? "5000"}");
Console.WriteLine("SignalR Hub: /vatsimhub");
Console.WriteLine("API Docs: /swagger");
Console.WriteLine("===========================================");
Console.WriteLine("");
Console.WriteLine("To start pairing:");
Console.WriteLine("  1. Start vPilot and connect to VATSIM");
Console.WriteLine("  2. Open mobile app");
Console.WriteLine("  3. Generate pairing code: POST /api/pairing/start");
Console.WriteLine("  4. Enter code in mobile app");
Console.WriteLine("===========================================");

app.Run();
return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
