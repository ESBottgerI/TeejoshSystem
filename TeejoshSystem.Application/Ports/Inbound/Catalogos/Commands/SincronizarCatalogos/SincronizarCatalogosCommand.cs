using MediatR;

namespace TeejoshSystem.Application.Ports.Inbound.Catalogos.Commands.SincronizarCatalogos
{
    public record SincronizarCatalogosCommand : IRequest<SincronizarCatalogosResult>;

    public record SincronizarCatalogosResult(
        int TotalAgregadas,
        int TotalActualizadas,
        List<string> Errores
    );
}