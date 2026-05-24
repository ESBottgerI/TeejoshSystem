using MediatR;

namespace TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerImagenExpansion
{
    public record ObtenerImagenExpansionQuery(int ExpansionId) : IRequest<string?>;
}