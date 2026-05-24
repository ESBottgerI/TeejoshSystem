using System.Net.Http.Json;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Apis
{
    public class TcgdexAdapter : ITcgCatalogoApiService
    {
        private readonly HttpClient _http;
        private readonly IAppLogger _logger;                 // NUEVO
        public string FranquiciaNombre => "Pokémon";

        public TcgdexAdapter(HttpClient http, IAppLogger logger)  // NUEVO
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<ExpansionApiResult>> GetExpansionesAsync()
        {
            _logger.Debug("TcgdexAdapter: consultando sets de Pokémon en api.tcgdex.net...");
            try
            {
                var sets = await _http.GetFromJsonAsync<List<TcgdexSet>>(
                    "https://api.tcgdex.net/v2/es/sets");

                if (sets is null)
                {
                    _logger.Warning("TcgdexAdapter: la API devolvió respuesta nula.");
                    return new();
                }

                _logger.Info($"TcgdexAdapter: {sets.Count} sets recibidos de TCGdex.");
                return sets.Select(s => new ExpansionApiResult(
                    Nombre: s.Name,
                    ImageUrl: s.Logo is not null ? $"{s.Logo}.png" : null
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("TcgdexAdapter: error al consultar la API de Pokémon.", ex);
                return new();
            }
        }

        private sealed class TcgdexSet
        {
            public string Id { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string? Logo { get; set; }
        }
    }
}