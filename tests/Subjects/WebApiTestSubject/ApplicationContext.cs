using Microsoft.EntityFrameworkCore;

namespace WebApiTestSubject;

public class WeatherForecastEntity
{
    public long Id { get; init; }
    public required WeatherForecast Data { get; init; }
}

public class ApplicationDbContext : DbContext
{
    public DbSet<WeatherForecastEntity> WeatherForecasts { get; init; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherForecastEntity>().ComplexProperty(e => e.Data);
    }
}