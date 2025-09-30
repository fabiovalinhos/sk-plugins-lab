using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace Chat.WebBlazorServer.Plugins
{
    public class GetWeather
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetWeather(IHttpClientFactory httpClientFactory) =>
            _httpClientFactory = httpClientFactory;

        [KernelFunction("get_weather_forecast")]
        [Description("obtenha a previsão do tempo de 7 dias para uma determinada latitude e longitude")]
        [return: Description("retorna a previsão de 7 dias em incrementos de 12 horas, formatada como Digital Weather Markup Language (SAML)")]
        public async Task<string> GetWeatherPointAsync(decimal latitude, decimal longitude)
        {
            var forecastUrl = await GetForecastURL(latitude, longitude)
                ?? throw new InvalidOperationException("URL de previsão não encontrada na resposta.");

            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, forecastUrl);
            request.Headers.Add("User-Agent", "myapplication-udemy-course");
            request.Headers.Add("accept", "application/vnd.noaa.dwml+xml");

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string?> GetForecastURL(decimal latitude, decimal longitude)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.weather.gov/points/{latitude},{longitude}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "myapplication-udemy-course");

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("properties", out var properties) &&
                properties.TryGetProperty("forecast", out var forecast))
            {
                return forecast.GetString();
            }
            return null;
        }
    }
}