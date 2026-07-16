using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Routing
{
    /// <summary>
    /// Repositorio de enrutamiento para Catálogos.
    ///
    /// Los catálogos son de solo lectura desde la tienda (Avalonia).
    /// Solo Blazor Admin puede modificar catálogos, por lo que no hay outbox para escrituras.
    ///
    /// Al reconectar, el SyncService hace un refresh completo de catálogos
    /// desde Supabase hacia SQLite local (full replace, no merge).
    /// </summary>
    public class RoutingCatalogoRepository : ICatalogoRepository
    {
        private readonly IConnectivityService _connectivity;
        private readonly InventarioDbContext _pgContext;
        private readonly InventarioDbContext _localContext;

        private CatalogoRepository PgRepo => new(_pgContext);
        private CatalogoRepository LocalRepo => new(_localContext);

        public RoutingCatalogoRepository(
            IConnectivityService connectivity,
            InventarioDbContext pgContext,
            InventarioDbContext localContext)
        {
            _connectivity = connectivity;
            _pgContext = pgContext;
            _localContext = localContext;
        }

        // Todos los métodos de lectura: online → PostgreSQL, offline → SQLite

        public Task<List<HotWheelsCategoria>> GetHotWheelsCategoriasAsync()
            => Active().GetHotWheelsCategoriasAsync();

        public Task<List<FunkoSubtipo>> GetFunkoSubtiposAsync()
            => Active().GetFunkoSubtiposAsync();

        public Task<List<FunkoCaracteristica>> GetFunkoCaracteristicasAsync()
            => Active().GetFunkoCaracteristicasAsync();

        public Task<List<TcgFranquicia>> GetTcgFranquiciasAsync()
            => Active().GetTcgFranquiciasAsync();

        public Task<List<TcgExpansion>> GetTcgExpansionesAsync(int franquiciaId)
            => Active().GetTcgExpansionesAsync(franquiciaId);

        public Task<List<TcgPack>> GetTcgPacksAsync(int franquiciaId)
            => Active().GetTcgPacksAsync(franquiciaId);

        public Task<TcgExpansion?> GetTcgExpansionByIdAsync(int expansionId)
            => Active().GetTcgExpansionByIdAsync(expansionId);

        public Task<TcgPack?> GetTcgPackByIdAsync(int packId)
            => Active().GetTcgPackByIdAsync(packId);

        public Task<TcgFranquicia?> GetTcgFranquiciaByNombreAsync(string nombre)
            => Active().GetTcgFranquiciaByNombreAsync(nombre);

        public Task<TcgExpansion?> GetTcgExpansionByNombreYFranquiciaAsync(
            string nombre, int franquiciaId)
            => Active().GetTcgExpansionByNombreYFranquiciaAsync(nombre, franquiciaId);

        // Escrituras de catálogos: siempre online (solo las ejecuta SincronizarCatalogosCommandHandler)
        public Task AddTcgExpansionAsync(TcgExpansion expansion)
            => _connectivity.IsOnline
                ? PgRepo.AddTcgExpansionAsync(expansion)
                : LocalRepo.AddTcgExpansionAsync(expansion);

        public Task UpdateTcgExpansionAsync(TcgExpansion expansion)
            => _connectivity.IsOnline
                ? PgRepo.UpdateTcgExpansionAsync(expansion)
                : LocalRepo.UpdateTcgExpansionAsync(expansion);

        private CatalogoRepository Active()
            => _connectivity.IsOnline ? PgRepo : LocalRepo;
    }
}