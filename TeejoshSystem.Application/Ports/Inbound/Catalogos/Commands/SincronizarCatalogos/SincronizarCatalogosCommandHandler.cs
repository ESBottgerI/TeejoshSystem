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

        public SincronizarCatalogosCommandHandler(
            ICatalogoRepository catalogoRepo,
            IEnumerable<ITcgCatalogoApiService> apiServices,
            IImageStorageService imageStorage)
        {
            _catalogoRepo = catalogoRepo;
            _apiServices = apiServices;
            _imageStorage = imageStorage;
        }

        public async Task<SincronizarCatalogosResult> Handle(
            SincronizarCatalogosCommand request,
            CancellationToken cancellationToken)
        {
            var errores = new List<string>();
            int totalAgregadas = 0;
            int totalActualizadas = 0;

            foreach (var servicio in _apiServices)
            {
                try
                {
                    var franquicia = await _catalogoRepo
                        .GetTcgFranquiciaByNombreAsync(servicio.FranquiciaNombre);

                    if (franquicia is null)
                    {
                        errores.Add($"Franquicia '{servicio.FranquiciaNombre}' no encontrada en BD.");
                        continue;
                    }

                    var expansionesApi = await servicio.GetExpansionesAsync();

                    foreach (var expansionApi in expansionesApi)
                    {
                        var existente = await _catalogoRepo
                            .GetTcgExpansionByNombreYFranquiciaAsync(
                                expansionApi.Nombre, franquicia.Id);

                        // Descargar imagen — SVG se convierte a PNG automáticamente
                        string? imageName = null;
                        if (expansionApi.ImageUrl is not null)
                            imageName = await _imageStorage
                                .SaveImageFromUrlAsync(expansionApi.ImageUrl);

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
                    errores.Add($"Error sincronizando {servicio.FranquiciaNombre}: {ex.Message}");
                }
            }

            return new SincronizarCatalogosResult(totalAgregadas, totalActualizadas, errores);
        }
    }
}