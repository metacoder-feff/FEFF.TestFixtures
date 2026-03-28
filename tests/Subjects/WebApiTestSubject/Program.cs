namespace WebApiTestSubject;

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
    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services
            .AddSingleton((_) => Random.Shared)
            .AddSingleton((_) => TimeProvider.System)
            .AddScoped<SomeService>();
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
                IsDevelopment   = c.IsDevelopment(),
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

        app.Run();
    }
}

/*
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        app.UseHttpsRedirection();

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        app.MapGet("/weatherforecast", () =>
        {
            var forecast =  Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        });
*/