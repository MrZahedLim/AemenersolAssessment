using AemenersolSync.Data;
using AemenersolSync.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// dotnet run            -> GetPlatformWellActual
// dotnet run -- --dummy -> GetPlatformWellDummy
var useDummy = args.Contains("--dummy", StringComparer.OrdinalIgnoreCase);

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHttpClient<AemenersolApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddScoped<SyncService>();

using var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    logger.LogInformation("Applying database migrations...");
    await db.Database.MigrateAsync();

    var sync = scope.ServiceProvider.GetRequiredService<SyncService>();
    logger.LogInformation("Starting sync (source: {Source})...", useDummy ? "Dummy" : "Actual");
    await sync.RunAsync(useDummy);

    logger.LogInformation("Done.");
    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Sync failed.");
    return 1;
}
