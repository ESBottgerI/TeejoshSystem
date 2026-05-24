using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Apis
{
    public class ScryfallAdapter : ITcgCatalogoApiService
    {
        private readonly HttpClient _http;
        private readonly IAppLogger _logger;                 // NUEVO
        public string FranquiciaNombre => "Magic: The Gathering";

        public ScryfallAdapter(HttpClient http, IAppLogger logger)  // NUEVO
        {
            _http = http;
            _logger = logger;
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.TryAddWithoutValidation(
                "User-Agent", "TeejoshSystem/1.0 (contact@teejosh.com)");
            _http.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<List<ExpansionApiResult>> GetExpansionesAsync()
        {
            _logger.Debug("ScryfallAdapter: consultando sets en api.scryfall.com...");
            try
            {
                var response = await _http.GetFromJsonAsync<ScryfallSetList>(
                    "https://api.scryfall.com/sets");

                if (response?.Data is null)
                {
                    _logger.Warning("ScryfallAdapter: la API devolvió respuesta nula o sin datos.");
                    return new();
                }

                var filtered = response.Data
                    .Where(s => s.SetType == "expansion" ||
                                s.SetType == "core" ||
                                s.SetType == "masters" ||
                                s.SetType == "draft_innovation" ||
                                s.SetType == "commander")
                    .ToList();

                _logger.Info($"ScryfallAdapter: {filtered.Count} sets filtrados de {response.Data.Count} totales.");

                return filtered.Select(s => new ExpansionApiResult(
                    Nombre: s.Name,
                    ImageUrl: s.IconSvgUri
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("ScryfallAdapter: error al consultar la API de Magic: The Gathering.", ex);
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