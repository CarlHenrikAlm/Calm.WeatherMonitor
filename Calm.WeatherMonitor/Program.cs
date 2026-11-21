using Calm.WeatherMonitor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;


var Configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var connectionString = 
    Configuration.GetConnectionString("DefaultConnection");

var options = new DbContextOptionsBuilder<WeatherDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using var db = new WeatherDbContext(options);

Console.WriteLine("Database connection established.");

