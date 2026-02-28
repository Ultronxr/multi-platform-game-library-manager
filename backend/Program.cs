using GameLibrary.Api.Data;
using GameLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using System.Text.Json.Serialization;

var bootstrapLogger = LogManager
    .Setup()
    .LoadConfigurationFromFile("nlog.config")
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    var connectionString = builder.Configuration.GetConnectionString("GameLibraryMySql")
        ?? throw new InvalidOperationException("Missing connection string: ConnectionStrings:GameLibraryMySql");

    builder.Services.AddDbContextFactory<GameLibraryDbContext>(options =>
    {
        options.UseMySql(
            connectionString,
            ServerVersion.Parse("5.7.44-mysql"));
    });

    builder.Services.AddScoped<IGameLibraryStore, EfCoreGameLibraryStore>();
    builder.Services.AddSingleton<DuplicateDetector>();
    builder.Services.AddHttpClient<SteamOwnedGamesClient>();
    builder.Services.AddHttpClient<EpicLibraryClient>();

    var app = builder.Build();

    app.UseCors("Frontend");
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    bootstrapLogger.Error(ex, "Application terminated unexpectedly.");
    throw;
}
finally
{
    LogManager.Shutdown();
}
