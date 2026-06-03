using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Domain.Tests.Entities.Detalles;

public class ProductoDetalleTests
{
    // ═════════════════════════════════════════════════════════════════════════
    // ProductoDetalle.AsignarProductoId
    // ═════════════════════════════════════════════════════════════════════════

    [Fact]
    public void AsignarProductoId_ConIdUno_DebeAsignarCorrectamente()
    {
        // Boundary inferior válido: 1
        var detalle = new TcgDetalle(1, 1);

        detalle.AsignarProductoId(1);

        detalle.ProductoId.Should().Be(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(int.MaxValue)]
    public void AsignarProductoId_ConIdPositivo_DebeAsignar(int id)
    {
        var detalle = new TcgDetalle(1, 1);

        detalle.AsignarProductoId(id);

        detalle.ProductoId.Should().Be(id);
    }

    [Fact]
    public void AsignarProductoId_ConCero_DebeArrojarArgumentException()
    {
        // Boundary: 0 inválido — mata el mutante <= → <
        var detalle = new TcgDetalle(1, 1);

        var act = () => detalle.AsignarProductoId(0);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*mayor a 0*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AsignarProductoId_ConNegativo_DebeArrojarArgumentException(int id)
    {
        var detalle = new TcgDetalle(1, 1);

        var act = () => detalle.AsignarProductoId(id);

        act.Should().Throw<ArgumentException>();
    }
}
