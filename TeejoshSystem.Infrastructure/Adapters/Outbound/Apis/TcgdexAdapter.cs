using System.Net.Http.Json;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Apis
{
    public class TcgdexAdapter : ITcgCatalogoApiService
    {
        private readonly HttpClient _http;
        public string FranquiciaNombre => "Pokémon";

        public TcgdexAdapter(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ExpansionApiResult>> GetExpansionesAsync()
        {
            try
            {
                var sets = await _http.GetFromJsonAsync<List<TcgdexSet>>(
                    "https://api.tcgdex.net/v2/es/sets");

                if (sets is null) return new();

                return sets.Select(s => new ExpansionApiResult(
                    Nombre: s.Name,
                    ImageUrl: s.Logo is not null ? $"{s.Logo}.png" : null
                )).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TcgdexAdapter error: {ex.Message}");
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