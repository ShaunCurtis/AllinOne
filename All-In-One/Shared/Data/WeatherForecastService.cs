using System;
using System.Linq;
using System.Threading.Tasks;

namespace AllinOne.Shared.Data
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private WeatherForecast[] Forecasts;

        public WeatherForecastService()
        {
            var rng = new Random();
            var startDate = DateTime.Now.AddDays(-14);
            Forecasts =  Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray();
        }

        public virtual Task<WeatherForecast[]> GetForecastAsync() => Task.FromResult(Forecasts);

    }
}
