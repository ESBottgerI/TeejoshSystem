using MediatR;
using TeejoshInventario.Domain.Ports.Outbound.Repositories;

namespace TeejoshInventario.Application.Ports.Inbound.Catalogos.Queries.ObtenerCatalogos
{
    public class ObtenerCatalogosQueryHandler
        : IRequestHandler<ObtenerCatalogosQuery, CatalogosDto>
    {
        private readonly ICatalogoRepository _repository;

        public ObtenerCatalogosQueryHandler(ICatalogoRepository repository)
        {
            _repository = repository;
        }

        public async Task<CatalogosDto> Handle(
            ObtenerCatalogosQuery request,
            CancellationToken cancellationToken)
        {
            return new CatalogosDto
            {
                CategoriasHotWheels = await _repository.GetHotWheelsCategoriasAsync(),
                SubtiposFunko = await _repository.GetFunkoSubtiposAsync(),
                CaracteristicasFunko = await _repository.GetFunkoCaracteristicasAsync(),
                FranquiciasTcg = await _repository.GetTcgFranquiciasAsync()
            };
        }
    }
}