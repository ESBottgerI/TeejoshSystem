using MediatR;

using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos
{
    public class BuscarProductosQueryHandler : IRequestHandler<BuscarProductosQuery, List<ProductoBusquedaDto>>
    {
        private readonly IProductoRepository _repository;

        public BuscarProductosQueryHandler(IProductoRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductoBusquedaDto>> Handle(
            BuscarProductosQuery request,
            CancellationToken cancellationToken)
        {
            var productos = await _repository.SearchAsync(request.Nombre, request.Tipo);

            return productos.Select(p => new ProductoBusquedaDto
            {
                Id = p.Id,
                Tipo = p.Tipo,
                Nombre = p.Nombre.Value,
                Precio = p.Precio.Value,
                Unidades = p.Stock.Value,
                DetalleResumen = GenerarResumen(p)
            }).ToList();
        }

        private static string GenerarResumen(Producto p) => p.Descripcion switch
        {
            HotWheelsDetalle hw => $"{hw.Modelo} · {hw.Anio} · {hw.Serie}",
            FunkoDetalle fu => $"#{fu.NumeroCaja} · {fu.Licencia}",
            TcgDetalle tcg => $"Pack {tcg.PackId} · Expansión {tcg.ExpansionId}",
            ToyDetalle toy => $"{toy.JugadoresMin}-{toy.JugadoresMax} jugadores",
            VariosDetalle v => $"{v.Marca} · {v.Material}",
            null => "Sin detalle",
            _ => "Detalle desconocido"
        };
    }
}
