using FluentAssertions;
using MediatR;
using NSubstitute;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Commands;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Queries;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Ventas;

namespace TeejoshSystem.AvaloniaUI.Tests.Ventas;

public class RegistrarVentaViewModelTests
{
    private readonly IMediator _mediator;
    private readonly RegistrarVentaViewModel _vm;

    public RegistrarVentaViewModelTests()
    {
        _mediator = Substitute.For<IMediator>();
        _vm = new RegistrarVentaViewModel(_mediator);
    }

    [Fact]
    public async Task ConfirmarVentaAsync_ConLineasValidas_DebeEnviarCommandCorreto()
    {
        _mediator
            .Send(Arg.Any<RegistrarVentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Agregar una línea al carrito del ViewModel
        _vm.AgregarLinea(productoId: 1, cantidad: 2, precioUnitario: 25m);

        await _vm.ConfirmarVentaAsync();

        await _mediator.Received(1).Send(
            Arg.Is<RegistrarVentaCommand>(c =>
                c.Lineas.Count == 1 &&
                c.Lineas[0].ProductoId == 1 &&
                c.Lineas[0].Cantidad == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmarVentaAsync_VentaExitosa_DebeLimpiarLineas()
    {
        _mediator
            .Send(Arg.Any<RegistrarVentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _vm.AgregarLinea(1, 1, 10m);
        await _vm.ConfirmarVentaAsync();

        _vm.Lineas.Should().BeEmpty();
    }

    [Fact]
    public async Task ConfirmarVentaAsync_VentaFallida_DebeConservarLineas()
    {
        _mediator
            .Send(Arg.Any<RegistrarVentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Stock insuficiente"));

        _vm.AgregarLinea(1, 99, 10m);
        await _vm.ConfirmarVentaAsync();

        // Las líneas se conservan para que el usuario pueda corregir
        _vm.Lineas.Should().HaveCount(1);
    }

    [Fact]
    public async Task ConfirmarVentaAsync_SinLineas_NoDebeEnviarCommand()
    {
        // Carrito vacío — no tiene sentido enviar una venta sin ítems
        await _vm.ConfirmarVentaAsync();

        await _mediator.DidNotReceive().Send(
            Arg.Any<RegistrarVentaCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void TotalVenta_DebeCalcularSumaDeTodosLosSubtotales()
    {
        _vm.AgregarLinea(1, cantidad: 2, precioUnitario: 10m); // 20
        _vm.AgregarLinea(2, cantidad: 3, precioUnitario: 15m); // 45

        _vm.TotalVenta.Should().Be(65m);
    }
}

public class HistorialVentasViewModelTests
{
    private readonly IMediator _mediator;
    private readonly HistorialVentasViewModel _vm;

    public HistorialVentasViewModelTests()
    {
        _mediator = Substitute.For<IMediator>();
        _vm = new HistorialVentasViewModel(_mediator);
    }

    [Fact]
    public async Task OnLoadedAsync_DebeCargarHistorialDeVentas()
    {
        var ventas = new List<VentaDto>
        {
            new() { Id = 1, Fecha = DateTime.Today, Total = 50m },
            new() { Id = 2, Fecha = DateTime.Today, Total = 30m }
        };

        _mediator
            .Send(Arg.Any<ObtenerVentasQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IEnumerable<VentaDto>>(ventas));

        await _vm.OnLoadedAsync();

        _vm.Ventas.Should().HaveCount(2);
    }

    [Fact]
    public async Task OnLoadedAsync_SinVentas_DebeDejarHistorialVacio()
    {
        _mediator
            .Send(Arg.Any<ObtenerVentasQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IEnumerable<VentaDto>>(Enumerable.Empty<VentaDto>()));

        await _vm.OnLoadedAsync();

        _vm.Ventas.Should().BeEmpty();
    }
}
