using MediatR;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerImagenExpansion
{
    public class ObtenerImagenExpansionQueryHandler
        : IRequestHandler<ObtenerImagenExpansionQuery, string?>
    {
        private readonly ICatalogoRepository _repository;

        public ObtenerImagenExpansionQueryHandler(ICatalogoRepository repository)
        {
            _repository = repository;
        }

        public async Task<string?> Handle(
            ObtenerImagenExpansionQuery request,
            CancellationToken cancellationToken)
        {
            var expansion = await _repository.GetTcgExpansionByIdAsync(request.ExpansionId);
            return expansion?.ImageUrl;
        }
    }
}