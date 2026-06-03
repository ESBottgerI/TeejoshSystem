using FluentAssertions;
using NSubstitute;
using Reqnroll;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Commands.RegistrarVenta;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Gherkin.StepDefinitions;

[Binding]
public class RegistrarVentaSteps
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly RegistrarVentaCommandHandler _handler;

    private readonly List<RegistrarVentaItemCommand> _items = [];

    private Result<int> _resultado = Result.Success(0);

    public RegistrarVentaSteps()
    {
        _ventaRepository = Substitute.For<IVentaRepository>();
        _productoRepository = Substitute.For<IProductoRepository>();

        _handler = new RegistrarVentaCommandHandler(
            _ventaRepository,
            _productoRepository);

        _ventaRepository
            .AddAsync(Arg.Any<Venta>())
            .Returns(1);
    }

    [Given("existe un producto con ID {int} llamado {string} con stock {int} y precio {int}")]
    public void GivenExisteUnProductoConID(
        int id,
        string nombre,
        int stock,
        int precio)
    {
        var producto = new Producto(
            TipoProducto.HotWheels,
            new NombreProducto(nombre),
            new Precio(precio),
            new Unidades(stock));

        // Asignar ID manualmente mediante reflection
        typeof(Producto)
            .GetProperty(nameof(Producto.Id))!
            .SetValue(producto, id);

        _productoRepository
            .GetByIdAsync(id)
            .Returns(producto);
    }

    [Given("no existe el producto con ID {int}")]
    public void GivenNoExisteElProductoConID(int id)
    {
        _productoRepository
            .GetByIdAsync(id)
            .Returns((Producto?)null);
    }

    [Given("se desea vender {int} unidades del producto {int}")]
    public void GivenSeDeseaVenderUnidadesDelProducto(
        int cantidad,
        int productoId)
    {
        _items.Add(new RegistrarVentaItemCommand(
            productoId,
            cantidad));
    }

    [When("el administrador registra la venta")]
    public async Task WhenElAdministradorRegistraLaVenta()
    {
        var command = new RegistrarVentaCommand(_items);

        _resultado = await _handler.Handle(
            command,
            CancellationToken.None);
    }

    [Then("la venta debe registrarse correctamente")]
    public void ThenLaVentaDebeRegistrarseCorrectamente()
    {
        _resultado.IsSuccess.Should().BeTrue();
    }

    [Then("el sistema debe rechazar la venta por stock insuficiente")]
    public void ThenStockInsuficiente()
    {
        _resultado.IsSuccess.Should().BeFalse();
    }

    [Then("el sistema debe rechazar la venta por producto inexistente")]
    public void ThenProductoInexistente()
    {
        _resultado.IsSuccess.Should().BeFalse();
    }
}