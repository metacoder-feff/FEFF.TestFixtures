using Microsoft.EntityFrameworkCore;

namespace WebApiTestSubject;

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);

public class SomeService(ILogger<SomeService> logger)
{
    public string Data => "123";

    public void Log()
    {
        logger.LogInformation("log-1");
        logger.LogWarning("log-2");
    }
}

public class Program
{
    /// <remarks>
    /// <list type="bullet">
    /// <item> For development:<br/>
    /// Postgres is started using "DevContainer" ("postgres" service).<br/>
    /// The connection string is defined as an environment variable for main container ("app" service).<br/>
    /// See <see href="https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/.devcontainer/devcontainer.json">.devcontainer/docker-compose.yml</see>.
    /// </item>
    /// <item> 
    /// For CI e.g. GitLab:<br/>
    /// Postgres is started as an additional service.<br/>
    /// The connection string is defined as an environment variable for test job.
    /// </item>
    /// </list>
    /// </remarks>
    public const string ConnectionStringName = "PgDb";


    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services
            .AddSingleton((_) => Random.Shared)
            .AddSingleton((_) => TimeProvider.System)
            .AddScoped<SomeService>()
            .AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                // options.UseNpgsql(builder.Configuration.GetConnectionString(ConnectionStringName));
                var connStr = sp
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString(ConnectionStringName)
                    ;
                options.UseNpgsql(connStr);
            });
        var app = builder.Build();

        app.MapGet("/weatherforecast/const", () =>
        {
            return new List<WeatherForecast>()
            {
                new (DateOnly.Parse("2000-01-01"), 20, "normal")
            };
        });

        app.MapGet("/weatherforecast/env", (IConfiguration c) =>
        {
            var summary = c.GetValue<string>("summary");
            return new List<WeatherForecast>()
            {
                new (DateOnly.Parse("2000-01-01"), 20, summary)
            };
        });

        app.MapGet("/asp-env", (IHostEnvironment c) =>
        {
            return new
            {
                c.ApplicationName,
                c.EnvironmentName,
                IsDevelopment = c.IsDevelopment(),
            };
        });

        app.MapGet("/weatherforecast/random", (Random r) =>
        {
            var t = r.Next(100);
            return new List<WeatherForecast>()
            {
                new (DateOnly.Parse("2000-01-01"), t, "normal")
            };
        });

        app.MapGet("/weatherforecast/time", (TimeProvider t) =>
        {
            var now = t.GetUtcNow();
            return new List<WeatherForecast>()
            {
                new (DateOnly.FromDateTime(now.Date) , 20, "normal")
            };
        });

        app.MapPost("/logging", (ILogger<Program> l, SomeService svc) =>
        {
            using var s = l.BeginScope("message-scope-1");
            svc.Log();
        });

        app.MapGet("/db-info", async (ApplicationDbContext dbCtx) =>
        {
            await dbCtx.Database.EnsureCreatedAsync();
            return new
            {
                Name = dbCtx.Database.GetDbConnection().Database
            };
        });

        app.MapPost("/weatherforecast", async (WeatherForecast forecast, ApplicationDbContext dbCtx) =>
        {
            dbCtx.WeatherForecasts.Add(new WeatherForecastEntity{Data = forecast});
            await dbCtx.SaveChangesAsync();
        });

        #region for Asp-tutorial

        app.MapPost("/weatherforecast/generate", async (TimeProvider tp, Random r, IConfiguration cfg, ApplicationDbContext dbCtx) =>
        {
            var now = tp.GetUtcNow();
            var date = DateOnly.FromDateTime(now.Date);
            var temperature = r.Next(100);
            var summary = cfg.GetValue<string>("summary");
            var forecast = new WeatherForecast(date, temperature, summary);
            dbCtx.WeatherForecasts.Add(new WeatherForecastEntity { Data = forecast });
            await dbCtx.SaveChangesAsync();
        });

        app.MapGet("/weatherforecast/today", async (TimeProvider tp, ApplicationDbContext dbCtx) =>
        {
            var now = tp.GetUtcNow();
            var today = DateOnly.FromDateTime(now.Date);

            var entity = await dbCtx.WeatherForecasts
                .Where(x => x.Data.Date == today)
                .FirstOrDefaultAsync();
                
            return entity is null ? Results.NotFound() : Results.Ok(entity.Data);
        });
        #endregion

        app.Run();
    }
}