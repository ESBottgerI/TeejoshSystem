using MediatR;
using TeejoshInventario.Domain.Entities.Catalogos;

namespace TeejoshInventario.Application.Ports.Inbound.Catalogos.Queries.ObtenerExpansionesYPacks
{
    public record ObtenerExpansionesYPacksQuery(int FranquiciaId)
        : IRequest<ExpansionesYPacksDto>;

    public class ExpansionesYPacksDto
    {
        public List<TcgExpansion> Expansiones { get; set; } = new();
        public List<TcgPack> Packs { get; set; } = new();
    }
}