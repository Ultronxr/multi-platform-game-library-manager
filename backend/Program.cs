using GameLibrary.Api.Data;
using GameLibrary.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using System.Text;
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
    builder.Services.AddEndpointsApiExplorer();

    var authOptions = builder.Configuration.GetSection("Auth").Get<AuthOptions>() ?? new AuthOptions();
    if (string.IsNullOrWhiteSpace(authOptions.SigningKey) || authOptions.SigningKey.Length < 32)
    {
        throw new InvalidOperationException("Auth:SigningKey must be configured and at least 32 characters long.");
    }

    builder.Services.AddSingleton(authOptions);
    builder.Services.AddSingleton<PasswordHashService>();
    builder.Services.AddSingleton<JwtTokenService>();

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SigningKey)),
                ValidateIssuer = true,
                ValidIssuer = authOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = authOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        // 默认要求所有接口登录，除非显式标记 [AllowAnonymous]。
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "GameLibrary API",
            Version = "v1"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "在此输入: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
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
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase),
        branch =>
        {
            branch.Use(async (context, next) =>
            {
                var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
                var swaggerEnabled = configuration.GetValue<bool>("Swagger:Enabled");
                if (!swaggerEnabled)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Swagger is disabled.");
                    return;
                }

                await next();
            });

            branch.UseSwagger();
            branch.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "GameLibrary API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "GameLibrary API Docs";
            });
        });
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
