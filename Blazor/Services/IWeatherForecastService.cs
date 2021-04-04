using Blazor.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazor.Services
{
    public interface IWeatherForecastService
    {
        public Task<List<WeatherForecast>> GetRecordsAsync();
    }
}
