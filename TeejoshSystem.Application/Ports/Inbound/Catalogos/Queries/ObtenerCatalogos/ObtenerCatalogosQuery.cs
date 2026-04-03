using MediatR;
using TeejoshSystem.Domain.Entities.Catalogos;

namespace TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerCatalogos
{
    public record ObtenerCatalogosQuery : IRequest<CatalogosDto>;

    public class CatalogosDto
    {
        public List<HotWheelsCategoria> CategoriasHotWheels { get; set; } = new();
        public List<FunkoSubtipo> SubtiposFunko { get; set; } = new();
        public List<FunkoCaracteristica> CaracteristicasFunko { get; set; } = new();
        public List<TcgFranquicia> FranquiciasTcg { get; set; } = new();
    }
}