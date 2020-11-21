using System.Threading.Tasks;

namespace AllinOne.Shared.Data
{
    public interface IWeatherForecastService
    {
        public Task<WeatherForecast[]> GetForecastAsync();
    }
}
