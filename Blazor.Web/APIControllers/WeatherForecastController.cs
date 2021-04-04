using System.Collections.Generic;
using System.Threading.Tasks;
using Blazor.Data;
using Microsoft.AspNetCore.Mvc;
using MVC = Microsoft.AspNetCore.Mvc;
using Blazor.Services;

namespace Blazor.Web.APIControllers
{
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        protected IWeatherForecastService DataService { get; set; }

        public WeatherForecastController(IWeatherForecastService dataService)
            => this.DataService = dataService;

        [MVC.Route("/api/weatherforecast/list")]
        [HttpGet]
        public async Task<List<WeatherForecast>> GetList() => await DataService.GetRecordsAsync();

    }
}
