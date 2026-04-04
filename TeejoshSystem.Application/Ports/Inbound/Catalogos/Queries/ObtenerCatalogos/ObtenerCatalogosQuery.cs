using MediatR;
using TeejoshSystem.Domain.Entities.Catalogos;

namespace TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerCatalogos
{
    public record ObtenerCatalogosQuery : IRequest<CatalogosDto>;
}