using Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Services
{
    public class WeatherForecastServerService : IWeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private List<WeatherForecast> records = new List<WeatherForecast>();

        public WeatherForecastServerService()
            => this.GetForecasts();

        public void GetForecasts()
        {
            var rng = new Random();
            records = Enumerable.Range(1, 10).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToList();
        }

        public Task<List<WeatherForecast>> GetRecordsAsync()
            => Task.FromResult(this.records);

    }
}
