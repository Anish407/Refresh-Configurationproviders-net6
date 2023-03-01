using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace RefreshIConfigurationproviders.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration configuration;
        public SettingsModel Model { get; set; }

        public WeatherForecastController(IOptionsSnapshot<SettingsModel> optionsSnapshot, IConfigurationRoot configurationRoot)
        {
            configurationRoot.Reload();
            Model = optionsSnapshot.Value;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public string Get()
        {

            return $"Name:{Model.name}: Secret:{Model.secret}";
        }
    }
}