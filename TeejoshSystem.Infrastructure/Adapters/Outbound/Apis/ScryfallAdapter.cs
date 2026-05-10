using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Apis
{
    public class ScryfallAdapter : ITcgCatalogoApiService
    {
        private readonly HttpClient _http;
        public string FranquiciaNombre => "Magic: The Gathering";

        public ScryfallAdapter(HttpClient http)
        {
            _http = http;
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.TryAddWithoutValidation(
                "User-Agent", "TeejoshSystem/1.0 (contact@teejosh.com)");
            _http.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<List<ExpansionApiResult>> GetExpansionesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ScryfallSetList>(
                    "https://api.scryfall.com/sets");

                if (response?.Data is null) return new();

                return response.Data
                    .Where(s => s.SetType == "expansion" ||
                                s.SetType == "core" ||
                                s.SetType == "masters" ||
                                s.SetType == "draft_innovation" ||
                                s.SetType == "commander")
                    .Select(s => new ExpansionApiResult(
                        Nombre: s.Name,
                        ImageUrl: s.IconSvgUri
                    )).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ScryfallAdapter error: {ex.Message}");
                return new();
            }
        }

        private sealed class ScryfallSetList
        {
            public List<ScryfallSet> Data { get; set; } = new();
        }

        private sealed class ScryfallSet
        {
            public string Name { get; set; } = null!;

            [JsonPropertyName("set_type")]
            public string SetType { get; set; } = null!;

            [JsonPropertyName("icon_svg_uri")]
            public string? IconSvgUri { get; set; }
        }
    }
}