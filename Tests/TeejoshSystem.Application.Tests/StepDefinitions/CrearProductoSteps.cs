using FluentAssertions;
using NSubstitute;
using Reqnroll;

using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.CrearProducto;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.StepDefinitions;

[Binding]
public class CrearProductoSteps
{
    private readonly IProductoRepository _repository;

    private readonly CrearProductoCommandHandler _handler;

    private readonly IImageStorageService _imageStorageMock;

    private readonly IAppLogger _applogger;

    private string _nombre = string.Empty;
    private decimal _precio;
    private int _stock;

    private bool _conDetalle = true;

    private Result _resultado = Result.Success();

    public CrearProductoSteps()
    {
        _repository = Substitute.For<IProductoRepository>();

        _imageStorageMock = Substitute.For<IImageStorageService>();

        _applogger = Substitute.For<IAppLogger>();

        _handler = new CrearProductoCommandHandler(
            _repository,
            _imageStorageMock,
            _applogger
        );

        _repository
            .AddAsync(Arg.Any<Domain.Entities.Producto>())
            .Returns(1);
    }

    [Given(@"existe un producto Hot Wheels llamado ""(.*)""")]
    public void GivenExisteProducto(string nombre)
    {
        _nombre = nombre;
    }

    [Given(@"el producto tiene precio (.*)")]
    public void GivenProductoTienePrecio(decimal precio)
    {
        _precio = precio;
    }

    [Given(@"el producto tiene stock (.*)")]
    public void GivenProductoTieneStock(int stock)
    {
        _stock = stock;
    }

    [Given(@"existe un producto sin detalle")]
    public void GivenProductoSinDetalle()
    {
        _conDetalle = false;
    }

    [When(@"el administrador registra el producto")]
    public async Task WhenAdministradorRegistraProducto()
    {
        var command = new CrearProductoCommand
        {
            Tipo = TipoProducto.HotWheels,
            Nombre = _nombre,
            Precio = _precio,
            Unidades = _stock,

            HotWheels = _conDetalle
                ? new CrearHotWheelsDetalleDto(
                    "Ford GT",
                    2024,
                    "Premium",
                    1)
                : null
        };

        _resultado = await _handler.Handle(
            command,
            CancellationToken.None);
    }

    [Then(@"el producto debe registrarse correctamente")]
    public void ThenProductoRegistrado()
    {
        _resultado.IsSuccess.Should().BeTrue();
    }

    [Then(@"el sistema debe rechazar el registro")]
    public void ThenSistemaRechaza()
    {
        _resultado.IsSuccess.Should().BeFalse();
    }
}