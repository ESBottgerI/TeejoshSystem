using MediatR;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Ports.Inbound.Catalogos.Commands.SincronizarCatalogos
{
    public class SincronizarCatalogosCommandHandler
        : IRequestHandler<SincronizarCatalogosCommand, SincronizarCatalogosResult>
    {
        private readonly ICatalogoRepository _catalogoRepo;
        private readonly IEnumerable<ITcgCatalogoApiService> _apiServices;
        private readonly IImageStorageService _imageStorage;
        private readonly IAppLogger _logger;                 // NUEVO

        public SincronizarCatalogosCommandHandler(
            ICatalogoRepository catalogoRepo,
            IEnumerable<ITcgCatalogoApiService> apiServices,
            IImageStorageService imageStorage,
            IAppLogger logger)                               // NUEVO
        {
            _catalogoRepo = catalogoRepo;
            _apiServices = apiServices;
            _imageStorage = imageStorage;
            _logger = logger;
        }

        public async Task<SincronizarCatalogosResult> Handle(
            SincronizarCatalogosCommand request,
            CancellationToken cancellationToken)
        {
            var errores = new List<string>();
            int totalAgregadas = 0;
            int totalActualizadas = 0;

            _logger.Info("Iniciando sincronización de catálogos TCG.");

            foreach (var servicio in _apiServices)
            {
                _logger.Debug($"Consultando API para franquicia '{servicio.FranquiciaNombre}'...");

                try
                {
                    var franquicia = await _catalogoRepo
                        .GetTcgFranquiciaByNombreAsync(servicio.FranquiciaNombre);

                    if (franquicia is null)
                    {
                        var msg = $"Franquicia '{servicio.FranquiciaNombre}' no encontrada en BD.";
                        _logger.Warning(msg);
                        errores.Add(msg);
                        continue;
                    }

                    var expansionesApi = await servicio.GetExpansionesAsync();
                    _logger.Debug($"API '{servicio.FranquiciaNombre}' devolvió {expansionesApi.Count} expansiones.");

                    foreach (var expansionApi in expansionesApi)
                    {
                        var existente = await _catalogoRepo
                            .GetTcgExpansionByNombreYFranquiciaAsync(
                                expansionApi.Nombre, franquicia.Id);

                        string? imageName = expansionApi.ImageUrl;

                        if (existente is null)
                        {
                            await _catalogoRepo.AddTcgExpansionAsync(new TcgExpansion
                            {
                                Nombre = expansionApi.Nombre,
                                FranquiciaId = franquicia.Id,
                                ImageUrl = imageName
                            });
                            totalAgregadas++;
                        }
                        else
                        {
                            bool cambio = false;
                            if (existente.Nombre != expansionApi.Nombre)
                            {
                                existente.Nombre = expansionApi.Nombre;
                                cambio = true;
                            }
                            if (imageName is not null && existente.ImageUrl != imageName)
                            {
                                existente.ImageUrl = imageName;
                                cambio = true;
                            }
                            if (cambio)
                            {
                                await _catalogoRepo.UpdateTcgExpansionAsync(existente);
                                totalActualizadas++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = $"Error sincronizando {servicio.FranquiciaNombre}: {ex.Message}";
                    _logger.Error($"Fallo al sincronizar catálogo de '{servicio.FranquiciaNombre}'", ex);
                    errores.Add(msg);
                }
            }

            _logger.Info($"Sincronización completada: {totalAgregadas} agregadas, {totalActualizadas} actualizadas, {errores.Count} errores.");

            return new SincronizarCatalogosResult(totalAgregadas, totalActualizadas, errores);
        }
    }
}