// Archivo: TeejoshSystem.Application/Ports/Inbound/Productos/Commands/CrearProducto/CrearProductoCommand.cs
using MediatR;

using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Application.Common;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Commands.CrearProducto
{
    public record CrearProductoCommand : IRequest<Result>
    {
        public TipoProducto Tipo { get; init; }
        public required string Nombre { get; init; }
        public decimal Precio { get; init; }
        public int Unidades { get; init; }
        public string? ImagePath { get; init; }  // NUEVO - ruta temporal del archivo origen

        public CrearHotWheelsDetalleDto? HotWheels { get; init; }
        public CrearFunkoDetalleDto? Funko { get; init; }
        public CrearTcgDetalleDto? Tcg { get; init; }
        public CrearToyDetalleDto? Toy { get; init; }
        public CrearVariosDetalleDto? Varios { get; init; }
    }

    public record CrearHotWheelsDetalleDto(
        string Modelo,
        int Anio,
        string Serie,
        int CategoriaId
    );

    public record CrearFunkoDetalleDto(
        int NumeroBox,
        string Licencia,
        int SubtipoId,
        int? CaracteristicaEspecialId
    );

    public record CrearTcgDetalleDto(
        int PackId,
        int ExpansionId
    );

    public record CrearToyDetalleDto(
        int EdadMinima,
        int JugadoresMinimo,
        int JugadoresMaximo,
        bool EsJuegoMesa
    );

    public record CrearVariosDetalleDto(
        string Marca,
        decimal Alto,
        decimal Ancho,
        decimal? Largo,
        string Material,
        bool TieneIlustracion
    );
}
