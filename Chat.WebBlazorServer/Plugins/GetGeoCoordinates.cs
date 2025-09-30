using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Chat.WebBlazorServer.Plugins
{
    public class GetGeoCoordinates
    {
        private IHttpClientFactory _httpClientFactory;
        private string _apiKey;

        public GetGeoCoordinates(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["GEOCODING_API_KEY"] ?? throw new MissingFieldException("GEOCODING_API_KEY");
        }


        [KernelFunction("geocode_address")]
        [Description("obtenha a latitude e longitude de um endereço")]
        [return: Description("o objeto JSON contêm os valores de lat e lon que correspondem ao endereço fornecido ou null se não for encontrado.")]
        public async Task<string?> GeocodeAddressAsync(string address)
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetStringAsync($"https://geocode.maps.co/search?q={address}&api_key={_apiKey}");

            using var doc = System.Text.Json.JsonDocument.Parse(response);
            var root = doc.RootElement;
            if (root.ValueKind != System.Text.Json.JsonValueKind.Array || root.GetArrayLength() == 0)
                return null;

            var first = root[0];
            if (first.TryGetProperty("lat", out var latProp) && first.TryGetProperty("lon", out var lonProp))
            {
                var result = new
                {
                    lat = latProp.GetString(),
                    lon = lonProp.GetString()
                };
                return System.Text.Json.JsonSerializer.Serialize(result);
            }
            return null;
        }
    }

}

