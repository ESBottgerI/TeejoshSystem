using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Apis
{
    public class YgoprodeckAdapter : ITcgCatalogoApiService
    {
        private readonly HttpClient _http;
        private readonly IAppLogger _logger;                 // NUEVO
        public string FranquiciaNombre => "Yu-Gi-Oh!";

        public YgoprodeckAdapter(HttpClient http, IAppLogger logger)  // NUEVO
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<ExpansionApiResult>> GetExpansionesAsync()
        {
            _logger.Debug("YgoprodeckAdapter: consultando cardsets en db.ygoprodeck.com...");
            try
            {
                var sets = await _http.GetFromJsonAsync<List<YgoSet>>(
                    "https://db.ygoprodeck.com/api/v7/cardsets.php");

                if (sets is null)
                {
                    _logger.Warning("YgoprodeckAdapter: la API devolvió respuesta nula.");
                    return new();
                }

                _logger.Info($"YgoprodeckAdapter: {sets.Count} sets recibidos de Ygoprodeck.");
                return sets.Select(s => new ExpansionApiResult(
                    Nombre: s.SetName,
                    ImageUrl: null
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("YgoprodeckAdapter: error al consultar la API de Yu-Gi-Oh!.", ex);
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