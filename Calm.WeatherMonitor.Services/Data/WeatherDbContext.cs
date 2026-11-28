using Calm.WeatherMonitor.Core.Entities;
using Microsoft.EntityFrameworkCore;



namespace Calm.WeatherMonitor.Services.Data;

public class WeatherDbContext : DbContext 
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) 
        : base(options)
    {
    }

    public DbSet<WeatherDataPoint> WeatherDataPoints =>
                Set<WeatherDataPoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherDataPoint>()
            .HasIndex(w => new
            {
                w.StationName,
                w.Timestamp
            })

         .IsUnique();
    }

}
