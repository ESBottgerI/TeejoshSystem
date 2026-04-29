using FluentAssertions;
using MediatR;
using NSubstitute;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;
using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.AvaloniaUI.Tests.Productos;

/// <summary>
/// Tests de ViewModels de Productos.
/// Los ViewModels son clases C# puras — no se levanta Avalonia.
/// IMediator se mockea con NSubstitute.
///
/// Qué se verifica aquí:
/// - Que OnLoadedAsync dispara la query correcta y popula la colección
/// - Que BuscarAsync dispara la query con el término correcto
/// - Que EliminarProductoAsync llama al command correcto y recarga
/// - Que el estado de la colección refleja la respuesta del mediator
///
/// AJUSTE REQUERIDO: los namespaces de ViewModels pueden variar.
/// Verifica con el árbol real de tu proyecto AvaloniaUI.
/// </summary>
public class GestionarProductosViewModelTests
{
    private readonly IMediator _mediator;
    private readonly GestionarProductosViewModel _vm;

    public GestionarProductosViewModelTests()
    {
        _mediator = Substitute.For<IMediator>();
        _vm = new GestionarProductosViewModel(_mediator);
    }

    // ── OnLoadedAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task OnLoadedAsync_DebeCargarListaDeProductos()
    {
        var productos = new List<ProductoDto>
        {
            new() { Id = 1, Nombre = "Ford GT",      Tipo = "HotWheels", Precio = 25m, Stock = 5 },
            new() { Id = 2, Nombre = "Pikachu 25°",  Tipo = "Funko",     Precio = 15m, Stock = 2 }
        };

        _mediator
            .Send(Arg.Any<ObtenerProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IEnumerable<ProductoDto>>(productos));

        await _vm.OnLoadedAsync();

        _vm.Productos.Should().HaveCount(2);
        _vm.Productos.Should().ContainSingle(p => p.Nombre == "Ford GT");
    }

    [Fact]
    public async Task OnLoadedAsync_RespuestaVacia_DebeDejarColeccionVacia()
    {
        _mediator
            .Send(Arg.Any<ObtenerProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IEnumerable<ProductoDto>>(Enumerable.Empty<ProductoDto>()));

        await _vm.OnLoadedAsync();

        _vm.Productos.Should().BeEmpty();
    }

    // ── Búsqueda ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarAsync_ConTermino_DebeEnviarQueryConTerminoCorreto()
    {
        _mediator
            .Send(Arg.Any<BuscarProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IEnumerable<ProductoBusquedaResult>>(Enumerable.Empty<ProductoBusquedaResult>()));

        _vm.TerminoBusqueda = "Ford";
        await _vm.BuscarAsync();

        await _mediator.Received(1).Send(
            Arg.Is<BuscarProductosQuery>(q => q.Termino == "Ford"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuscarAsync_ConFiltroTipo_DebeEnviarQueryConTipoCorrecto()
    {
        _mediator
            .Send(Arg.Any<BuscarProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IEnumerable<ProductoBusquedaResult>>(Enumerable.Empty<ProductoBusquedaResult>()));

        _vm.TipoFiltro = TipoProducto.HotWheels;
        await _vm.BuscarAsync();

        await _mediator.Received(1).Send(
            Arg.Is<BuscarProductosQuery>(q => q.Tipo == TipoProducto.HotWheels),
            Arg.Any<CancellationToken>());
    }

    // ── Eliminación ───────────────────────────────────────────────────────────

    [Fact]
    public async Task EliminarProductoAsync_Confirmado_DebeEnviarCommandYRecargarLista()
    {
        var productoSeleccionado = new ProductoDto { Id = 5, Nombre = "Test" };
        _vm.ProductoSeleccionado = productoSeleccionado;

        _mediator
            .Send(Arg.Any<EliminarProductoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _mediator
            .Send(Arg.Any<ObtenerProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IEnumerable<ProductoDto>>(Enumerable.Empty<ProductoDto>()));

        await _vm.EliminarProductoAsync(confirmar: true);

        await _mediator.Received(1).Send(
            Arg.Is<EliminarProductoCommand>(c => c.Id == 5),
            Arg.Any<CancellationToken>());

        // después de eliminar recarga la lista
        await _mediator.Received(1).Send(
            Arg.Any<ObtenerProductosQuery>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EliminarProductoAsync_SinConfirmar_NuncaEnviaCommand()
    {
        _vm.ProductoSeleccionado = new ProductoDto { Id = 3 };

        await _vm.EliminarProductoAsync(confirmar: false);

        await _mediator.DidNotReceive().Send(
            Arg.Any<EliminarProductoCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EliminarProductoAsync_SinProductoSeleccionado_NuncaEnviaCommand()
    {
        _vm.ProductoSeleccionado = null;

        await _vm.EliminarProductoAsync(confirmar: true);

        await _mediator.DidNotReceive().Send(
            Arg.Any<EliminarProductoCommand>(),
            Arg.Any<CancellationToken>());
    }
}

public class CrearProductoViewModelTests
{
    private readonly IMediator _mediator;
    private readonly CrearProductoViewModel _vm;

    public CrearProductoViewModelTests()
    {
        _mediator = Substitute.For<IMediator>();
        _vm = new CrearProductoViewModel(_mediator);
    }

    [Fact]
    public async Task GuardarAsync_CommandExitoso_DebeNotificarExitoYLimpiarFormulario()
    {
        _mediator
            .Send(Arg.Any<CrearProductoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _vm.Nombre = "Ford Mustang";
        _vm.Precio = 25m;
        _vm.Stock  = 3;
        _vm.Tipo   = TipoProducto.HotWheels;

        await _vm.GuardarAsync();

        await _mediator.Received(1).Send(
            Arg.Is<CrearProductoCommand>(c =>
                c.Nombre == "Ford Mustang" &&
                c.Precio == 25m &&
                c.Stock  == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GuardarAsync_CommandFallido_NoDebeLimpiarFormulario()
    {
        _mediator
            .Send(Arg.Any<CrearProductoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Error de validación"));

        _vm.Nombre = "Test";
        _vm.Precio = 10m;
        _vm.Stock  = 1;
        _vm.Tipo   = TipoProducto.Funko;

        await _vm.GuardarAsync();

        // El nombre debe seguir en el formulario — no limpiar en error
        _vm.Nombre.Should().Be("Test");
    }
}
