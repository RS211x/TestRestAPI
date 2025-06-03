using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Diagnostics.Eventing.Reader;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching", "Hotter than hellfire", "Infernal", "Lava-like", "Boiling"
};

var sql = @"CREATE TABLE IF NOT EXISTS WeatherForecast (
    Date DATE NOT NULL,
    TemperatureC INT NOT NULL,
    Summary NVARCHAR(100) NOT NULL,
    TemperatureF INT NOT NULl
);";

using var connection = new SqliteConnection(@"Data Source=/Users/rohanshrestha/C#/WEX/WeatherForecast.db");
connection.Open();
using var command = new SqliteCommand(sql, connection);
command.ExecuteNonQuery();
Console.WriteLine("Table created successfully.");

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-100, 100),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;

})
.WithName("GetWeatherForecast")
.WithOpenApi();

List<WeatherEntry> weatherData = await FetchWeatherDataAsync();


foreach (var entry in WeatherData.WeatherForecasts)
{
    var cmd = connection.CreateCommand();
    cmd.CommandText =
    @"INSERT INTO WeatherForecast (Date, Temperature, Condition)
      VALUES ($date, $temp, $cond)
      ON CONFLICT(Date) DO UPDATE SET
      Temperature = excluded.Temperature,
      Condition = excluded.Condition;
                ";
    cmd.Parameters.AddWithValue("$date", entry.Date);
    cmd.Parameters.AddWithValue("$temp", entry.Temperature);
    cmd.Parameters.AddWithValue("$cond", entry.Condition);

}


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}





