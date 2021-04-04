using Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;

namespace Blazor.Services
{
    public class WeatherForecastAPIService : IWeatherForecastService
    {

        protected HttpClient HttpClient { get; set; }

        public WeatherForecastAPIService(HttpClient httpClient)
            => this.HttpClient = httpClient;

        public async Task<List<WeatherForecast>> GetRecordsAsync()
            => await this.HttpClient.GetFromJsonAsync<List<WeatherForecast>>($"/api/weatherforecast/list");

    }
}
