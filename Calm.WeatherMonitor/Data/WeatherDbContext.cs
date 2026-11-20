using Calm.WeatherMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;


namespace Calm.WeatherMonitor.Data;

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
