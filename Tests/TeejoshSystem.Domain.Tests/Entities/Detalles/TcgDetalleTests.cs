using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Domain.Tests.Entities.Detalles;

public class TcgDetalleTests
{
    // ═════════════════════════════════════════════════════════════════════════
    // TcgDetalle — sin guards aritméticos, tests de asignación y Actualizar
    // ═════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Tcg_Constructor_DebeAsignarPackIdYExpansionId()
    {
        var detalle = new TcgDetalle(10, 25);

        detalle.PackId.Should().Be(10);
        detalle.ExpansionId.Should().Be(25);
    }

    [Fact]
    public void Tcg_Constructor_ConValoresDistintos_DebeAsignarCorrectamente()
    {
        var detalle = new TcgDetalle(99, 1);

        detalle.PackId.Should().Be(99);
        detalle.ExpansionId.Should().Be(1);
    }

    [Fact]
    public void Tcg_Actualizar_DebeModificarPackIdYExpansionId()
    {
        var detalle = new TcgDetalle(10, 25);

        detalle.Actualizar(50, 100);

        detalle.PackId.Should().Be(50);
        detalle.ExpansionId.Should().Be(100);
    }

    [Fact]
    public void Tcg_Actualizar_DebePoderAsignarMismoValor()
    {
        // Actualizar con los mismos valores no debe lanzar
        var detalle = new TcgDetalle(10, 25);

        var act = () => detalle.Actualizar(10, 25);

        act.Should().NotThrow();
    }
}
