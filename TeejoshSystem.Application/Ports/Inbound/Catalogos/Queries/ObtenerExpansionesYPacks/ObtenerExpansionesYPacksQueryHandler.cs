using MediatR;
using TeejoshInventario.Domain.Ports.Outbound.Repositories;

namespace TeejoshInventario.Application.Ports.Inbound.Catalogos.Queries.ObtenerExpansionesYPacks
{
    public class ObtenerExpansionesYPacksQueryHandler
        : IRequestHandler<ObtenerExpansionesYPacksQuery, ExpansionesYPacksDto>
    {
        private readonly ICatalogoRepository _repository;

        public ObtenerExpansionesYPacksQueryHandler(ICatalogoRepository repository)
        {
            _repository = repository;
        }

        public async Task<ExpansionesYPacksDto> Handle(
            ObtenerExpansionesYPacksQuery request,
            CancellationToken cancellationToken)
        {
            return new ExpansionesYPacksDto
            {
                Expansiones = await _repository.GetTcgExpansionesAsync(request.FranquiciaId),
                Packs = await _repository.GetTcgPacksAsync(request.FranquiciaId)
            };
        }
    }
}