using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Apis
{
    public class YgoprodeckAdapter : ITcgCatalogoApiService
    {
        private readonly HttpClient _http;
        public string FranquiciaNombre => "Yu-Gi-Oh!";

        public YgoprodeckAdapter(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ExpansionApiResult>> GetExpansionesAsync()
        {
            try
            {
                var sets = await _http.GetFromJsonAsync<List<YgoSet>>(
                    "https://db.ygoprodeck.com/api/v7/cardsets.php");

                if (sets is null) return new();

                return sets.Select(s => new ExpansionApiResult(
                    Nombre: s.SetName,
                    ImageUrl: null
                )).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"YgoprodeckAdapter error: {ex.Message}");
                return new();
            }
        }

        private sealed class YgoSet
        {
            [JsonPropertyName("set_name")]
            public string SetName { get; set; } = null!;

            [JsonPropertyName("set_code")]
            public string SetCode { get; set; } = null!;
        }
    }
}