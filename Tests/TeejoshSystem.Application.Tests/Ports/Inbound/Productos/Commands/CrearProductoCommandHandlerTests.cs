using System.Reflection.Metadata;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.CrearProducto;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Productos.Commands;

// ═══════════════════════════════════════════════════════════════════════════
// CrearProductoCommandHandler
// Retorna Result (no Result<T>)
// El detalle se pasa en propiedades anidadas (HotWheels, Funko, etc.)
// ═══════════════════════════════════════════════════════════════════════════

public class CrearProductoCommandHandlerTests
{
    /*
    private readonly IProductoRepository _repository;
    private readonly IImageStorageService _imageStorageMock;
    private readonly IAppLogger _applogger;

    private readonly CrearProductoCommandHandler _handler;

    public CrearProductoCommandHandlerTests()
    {
        _repository = Substitute.For<IProductoRepository>();
        _imageStorageMock = Substitute.For<IImageStorageService>();
        _applogger = Substitute.For<IAppLogger>();

        _handler = new CrearProductoCommandHandler(
            _repository,
            _imageStorageMock,
            _applogger
        );

    }
    */

    private readonly IProductoRepository _repository = Substitute.For<IProductoRepository>();
    private readonly IImageStorageService _imageStorage = Substitute.For<IImageStorageService>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    public CrearProductoCommandHandlerTests()
    {
        _repository = Substitute.For<IProductoRepository>();
        _imageStorage = Substitute.For<IImageStorageService>();
        _logger = Substitute.For<IAppLogger>();
    }

    private CrearProductoCommandHandler CrearHandler()
        => new(_repository, _imageStorage, _logger);

    // ── HotWheels — happy path y null guard ──────────────────────────────────────

    [Fact]
    public async Task Handle_CommandHotWheelsValido_DebeGuardarYPersistirDetalleCorrectamente()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>())
            .Returns((string?)null);

        _repository.AddAsync(Arg.Any<Producto>())
            .Returns(1);

        HotWheelsDetalle? detalleCapturado = null;

        _repository.AddHotWheelsDetalleAsync(
            Arg.Do<HotWheelsDetalle>(x => detalleCapturado = x))
            .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = TipoProducto.HotWheels,
                Nombre = "Camaro 1969",
                Precio = 25m,
                Unidades = 3,
                HotWheels = new CrearHotWheelsDetalleDto(
                    "Camaro",
                    2020,
                    "Muscle Mania",
                    1)
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        await _repository.Received(1)
            .AddHotWheelsDetalleAsync(Arg.Any<HotWheelsDetalle>());

        detalleCapturado.Should().NotBeNull();

        detalleCapturado!.Modelo.Should().Be("Camaro");
        detalleCapturado.Anio.Should().Be(2020);
        detalleCapturado.Serie.Should().Be("Muscle Mania");
        detalleCapturado.ProductoId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_HotWheels_LlamaRepositorioConValoresExactos()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>())
            .Returns((string?)null);

        _repository.AddAsync(Arg.Any<Producto>())
            .Returns(99);

        _repository.AddHotWheelsDetalleAsync(
            Arg.Any<HotWheelsDetalle>())
            .Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = TipoProducto.HotWheels,
                Nombre = "Ferrari",
                Precio = 25m,
                Unidades = 1,
                HotWheels = new CrearHotWheelsDetalleDto(
                    "Ferrari GTO",
                    2020,
                    "TH",
                    1)
            },
            CancellationToken.None);

        await _repository.Received(1)
            .AddHotWheelsDetalleAsync(
                Arg.Is<HotWheelsDetalle>(x =>
                    x.Modelo == "Ferrari GTO" &&
                    x.Anio == 2020 &&
                    x.Serie == "TH" &&
                    x.ProductoId == 99));
    }

    [Fact]
    public async Task Handle_HappyPath_RegistraLogsEsperados()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>())
            .Returns((string?)null);

        _repository.AddAsync(Arg.Any<Producto>())
            .Returns(123);

        _repository.AddHotWheelsDetalleAsync(
            Arg.Any<HotWheelsDetalle>())
            .Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = TipoProducto.HotWheels,
                Nombre = "Ferrari",
                Precio = 10,
                Unidades = 1,
                HotWheels = new CrearHotWheelsDetalleDto(
                    "F40",
                    2020,
                    "Premium",
                    1)
            },
            CancellationToken.None);

        _logger.Received(1)
            .Debug(Arg.Is<string>(x =>
                x.Contains("Tipo=HotWheels") &&
                x.Contains("Nombre=Ferrari")));

        _logger.Received(1)
            .Info(Arg.Is<string>(x =>
                x.Contains("Id=123") &&
                x.Contains("Nombre=Ferrari")));
    }

    [Fact]
    public async Task Handle_HotWheelsNull_RegistraWarningYRetornaError()
    {
        var result = await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = TipoProducto.HotWheels,
                Nombre = "Ferrari",
                Precio = 20,
                Unidades = 1,
                HotWheels = null
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();

        _logger.Received(1)
            .Warning(Arg.Is<string>(m =>
                m.Contains("Ferrari") &&
                m.Contains("Hot Wheels")));
    }

    [Fact]
    public async Task Handle_InvalidOperationException_RetornaFailure()
    {
        _imageStorage
            .When(x => x.SaveImageAsync(Arg.Any<string?>()))
            .Do(_ => throw new InvalidOperationException("Inválido"));

        var result = await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = TipoProducto.HotWheels,
                Nombre = "Ferrari",
                Precio = 10,
                Unidades = 1,
                HotWheels = new CrearHotWheelsDetalleDto(
                    "F40",
                    2020,
                    "Premium",
                    1)
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();

        _logger.Received(1)
            .Warning(Arg.Is<string>(x =>
                x.Contains("Inválido")));
    }

    [Fact]
    public async Task Handle_DebeGuardarImagenConRutaOriginal()
    {
        _imageStorage.SaveImageAsync("foto.png")
            .Returns("guardada.png");

        _repository.AddAsync(Arg.Any<Producto>())
            .Returns(1);

        _repository.AddHotWheelsDetalleAsync(
            Arg.Any<HotWheelsDetalle>())
            .Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = TipoProducto.HotWheels,
                Nombre = "Ferrari",
                Precio = 10,
                Unidades = 1,
                ImagePath = "foto.png",
                HotWheels = new CrearHotWheelsDetalleDto(
                    "F40",
                    2020,
                    "Premium",
                    1)
            },
            CancellationToken.None);

        await _imageStorage.Received(1)
            .SaveImageAsync("foto.png");
    }

    [Fact]
    public async Task Handle_CreaProductoConDatosCorrectos()
    {
        Producto? producto = null;

        _repository.AddAsync(
            Arg.Do<Producto>(p => producto = p))
            .Returns(1);

        _repository.AddHotWheelsDetalleAsync(
            Arg.Any<HotWheelsDetalle>())
            .Returns(Task.CompletedTask);

        _imageStorage.SaveImageAsync(Arg.Any<string?>())
            .Returns((string?)null);

        await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = TipoProducto.HotWheels,
                Nombre = "Ferrari",
                Precio = 50m,
                Unidades = 8,
                HotWheels = new CrearHotWheelsDetalleDto(
                    "F40",
                    2020,
                    "Premium",
                    1)
            },
            CancellationToken.None);

        producto.Should().NotBeNull();

        producto!.Tipo.Should().Be(TipoProducto.HotWheels);
        producto.Nombre.Value.Should().Be("Ferrari");
        producto.Precio.Value.Should().Be(50m);
        producto.Stock.Value.Should().Be(8);
    }

    /*
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_NombreInvalido_DebeRetornarFailureSinPersistir(string nombre)
    {
        var command = new CrearProductoCommand
        {
            Tipo = TipoProducto.HotWheels,
            Nombre = nombre,
            Precio = 15m,
            Unidades = 1,
            HotWheels = new CrearHotWheelsDetalleDto("Camaro", 2020, "Test", 1)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        await _repository.DidNotReceive().AddAsync(Arg.Any<Producto>());
    }

    [Fact]
    public async Task Handle_PrecioNegativo_DebeRetornarFailureSinPersistir()
    {
        var command = new CrearProductoCommand
        {
            Tipo = TipoProducto.HotWheels,
            Nombre = "Test",
            Precio = -5m,
            Unidades = 1,
            HotWheels = new CrearHotWheelsDetalleDto("Camaro", 2020, "Test", 1)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repository.DidNotReceive().AddAsync(Arg.Any<Producto>());
    }
    */

    // ═══════════════════════════════════════════════════════════════════════════
    // CrearProductoCommandHandler — survivors
    // Constructor: (IProductoRepository, IImageStorageService, IAppLogger)
    // ═══════════════════════════════════════════════════════════════════════════

    // ── Funko — happy path y null guard ──────────────────────────────────────

    [Fact]
    public async Task Handle_Funko_Valido_RetornaSuccessYLlamaAddFunkoDetalleAsync()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(1);
        _repository.AddFunkoDetalleAsync(Arg.Any<FunkoDetalle>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Funko,
            Nombre = "Pikachu 25°",
            Precio = 15m,
            Unidades = 2,
            Funko = new CrearFunkoDetalleDto(500, "Pokémon", 1, null)
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddFunkoDetalleAsync(Arg.Any<FunkoDetalle>());
    }

    [Fact]
    public async Task Handle_Funko_DetalleNull_RetornaFailureSinAgregarDetalle()
    {
        // Mata mutante: request.Funko is null → false
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(1);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Funko,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            Funko = null
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repository.DidNotReceive().AddFunkoDetalleAsync(Arg.Any<FunkoDetalle>());
    }

    [Fact]
    public async Task Handle_Funko_GuardaTodosLosCamposCorrectamente()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>())
            .Returns((string?)null);

        _repository.AddAsync(Arg.Any<Producto>())
            .Returns(77);

        FunkoDetalle? detalle = null;

        _repository.AddFunkoDetalleAsync(
            Arg.Do<FunkoDetalle>(x => detalle = x))
            .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = TipoProducto.Funko,
                Nombre = "Pikachu",
                Precio = 20m,
                Unidades = 5,
                Funko = new CrearFunkoDetalleDto(
                    500,
                    "Pokemon",
                    1,
                    null)
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        detalle.Should().NotBeNull();

        detalle!.ProductoId.Should().Be(77);
        detalle.NumeroCaja.Should().Be(500);
        detalle.Licencia.Should().Be("Pokemon");
    }

    // ── TCG ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Tcg_Valido_RetornaSuccessYLlamaAddTcgDetalleAsync()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(2);
        _repository.AddTcgDetalleAsync(Arg.Any<TcgDetalle>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Tcg,
            Nombre = "Booster Base Set",
            Precio = 8m,
            Unidades = 10,
            Tcg = new CrearTcgDetalleDto(1, 1)
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddTcgDetalleAsync(Arg.Any<TcgDetalle>());
    }

    [Fact]
    public async Task Handle_Tcg_DetalleNull_RetornaFailure()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(2);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Tcg,
            Nombre = "Test",
            Precio = 8m,
            Unidades = 1,
            Tcg = null
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repository.DidNotReceive().AddTcgDetalleAsync(Arg.Any<TcgDetalle>());
    }

    // ── Toy ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Toy_Valido_RetornaSuccessYLlamaAddToyDetalleAsync()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(3);
        _repository.AddToyDetalleAsync(Arg.Any<ToyDetalle>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Toy,
            Nombre = "Monopoly Clásico",
            Precio = 45m,
            Unidades = 5,
            Toy = new CrearToyDetalleDto(8, 2, 6, true)
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddToyDetalleAsync(Arg.Any<ToyDetalle>());
    }

    [Fact]
    public async Task Handle_Toy_DetalleNull_RetornaFailure()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(3);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Toy,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            Toy = null
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repository.DidNotReceive().AddToyDetalleAsync(Arg.Any<ToyDetalle>());
    }

    [Fact]
    public async Task Handle_Toy_GuardaTodosLosCamposCorrectamente()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>())
            .Returns((string?)null);

        _repository.AddAsync(Arg.Any<Producto>())
            .Returns(10);

        ToyDetalle? detalle = null;

        _repository.AddToyDetalleAsync(
            Arg.Do<ToyDetalle>(x => detalle = x))
            .Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = TipoProducto.Toy,
                Nombre = "Monopoly",
                Precio = 45m,
                Unidades = 5,
                Toy = new CrearToyDetalleDto(
                    8,
                    2,
                    6,
                    true)
            },
            CancellationToken.None);

        detalle.Should().NotBeNull();

        detalle!.ProductoId.Should().Be(10);
        detalle.EdadMinima.Should().Be(8);
        detalle.JugadoresMin.Should().Be(2);
        detalle.JugadoresMax.Should().Be(6);
        detalle.EsJuegoDeMesa.Should().BeTrue();
    }

    // ── Varios ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Varios_Valido_RetornaSuccessYLlamaAddVariosDetalleAsync()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(4);
        _repository.AddVariosDetalleAsync(Arg.Any<VariosDetalle>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Varios,
            Nombre = "Figura Bandai",
            Precio = 35m,
            Unidades = 3,
            Varios = new CrearVariosDetalleDto("Bandai", 15m, 10m, null, "PVC", false)
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddVariosDetalleAsync(Arg.Any<VariosDetalle>());
    }

    [Fact]
    public async Task Handle_Varios_DetalleNull_RetornaFailure()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(4);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Varios,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            Varios = null
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repository.DidNotReceive().AddVariosDetalleAsync(Arg.Any<VariosDetalle>());
    }

    [Fact]
    public async Task Handle_Varios_GuardaTodosLosCamposCorrectamente()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>())
            .Returns((string?)null);

        _repository.AddAsync(Arg.Any<Producto>())
            .Returns(11);

        VariosDetalle? detalle = null;

        _repository.AddVariosDetalleAsync(
            Arg.Do<VariosDetalle>(x => detalle = x))
            .Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = TipoProducto.Varios,
                Nombre = "Figura",
                Precio = 35m,
                Unidades = 2,
                Varios = new CrearVariosDetalleDto(
                    "Bandai",
                    15m,
                    10m,
                    null,
                    "PVC",
                    false)
            },
            CancellationToken.None);

        detalle.Should().NotBeNull();

        detalle!.ProductoId.Should().Be(11);
        detalle.Marca.Should().Be("Bandai");
        detalle.Alto.Should().Be(15m);
        detalle.Ancho.Should().Be(10m);
        detalle.Material.Should().Be("PVC");
    }

    // ── HotWheels null guard ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_HotWheels_DetalleNull_RetornaFailure()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(5);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.HotWheels,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            HotWheels = null
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repository.DidNotReceive().AddHotWheelsDetalleAsync(Arg.Any<HotWheelsDetalle>());
    }

    // ── productoId de AddAsync pasado a AsignarProductoId ────────────────────

    [Fact]
    public async Task Handle_HotWheels_AsignarProductoId_UsaElIdDeAddAsync()
    {
        // Mata mutante: AsignarProductoId(productoId) → AsignarProductoId(0)
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(99);

        HotWheelsDetalle? capturado = null;
        _repository.AddHotWheelsDetalleAsync(Arg.Do<HotWheelsDetalle>(d => capturado = d))
             .Returns(Task.CompletedTask);

        await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.HotWheels,
            Nombre = "Ferrari",
            Precio = 25m,
            Unidades = 1,
            HotWheels = new CrearHotWheelsDetalleDto("Ferrari GTO", 2020, "TH", 1)
        }, CancellationToken.None);

        capturado!.ProductoId.Should().Be(99);
    }

    [Fact]
    public async Task Handle_Funko_AsignarProductoId_UsaElIdDeAddAsync()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repository.AddAsync(Arg.Any<Producto>()).Returns(77);

        FunkoDetalle? capturado = null;
        _repository.AddFunkoDetalleAsync(Arg.Do<FunkoDetalle>(d => capturado = d))
             .Returns(Task.CompletedTask);

        await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Funko,
            Nombre = "Pikachu",
            Precio = 15m,
            Unidades = 1,
            Funko = new CrearFunkoDetalleDto(500, "Pokémon", 1, null)
        }, CancellationToken.None);

        capturado!.ProductoId.Should().Be(77);
    }

    // ── imageName is not null — par ambas ramas ───────────────────────────────

    [Fact]
    public async Task Handle_ImageNameNoNull_AsignaImagePathAlProductoAntesDeGuardar()
    {
        // Mata mutante: imageName is not null → false (nunca asigna)
        _imageStorage.SaveImageAsync("origen/img.jpg").Returns("guardada/img_001.jpg");

        Producto? capturado = null;
        _repository.AddAsync(Arg.Do<Producto>(p => capturado = p)).Returns(1);
        _repository.AddHotWheelsDetalleAsync(Arg.Any<HotWheelsDetalle>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.HotWheels,
            Nombre = "Supra",
            Precio = 25m,
            Unidades = 1,
            ImagePath = "origen/img.jpg",
            HotWheels = new CrearHotWheelsDetalleDto("Supra", 2020, "Basic", 1)
        }, CancellationToken.None);

        capturado!.ImagePath.Should().Be("guardada/img_001.jpg");
    }

    [Fact]
    public async Task Handle_ImageNameNull_NoAsignaImagePath()
    {
        // Mata mutante: imageName is not null → true (siempre asigna)
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);

        Producto? capturado = null;
        _repository.AddAsync(Arg.Do<Producto>(p => capturado = p)).Returns(1);
        _repository.AddHotWheelsDetalleAsync(Arg.Any<HotWheelsDetalle>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.HotWheels,
            Nombre = "Supra",
            Precio = 25m,
            Unidades = 1,
            ImagePath = null,
            HotWheels = new CrearHotWheelsDetalleDto("Supra", 2020, "Basic", 1)
        }, CancellationToken.None);

        capturado!.ImagePath.Should().BeNull();
    }

    // ── catch Exception genérica ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExcepcionGenerica_RetornaFailure()
    {
        _imageStorage.When(x => x.SaveImageAsync(Arg.Any<string?>()))
                     .Throw(new Exception("Error de IO"));

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.HotWheels,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            HotWheels = new CrearHotWheelsDetalleDto("Modelo", 2020, "Serie", 1)
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_TipoNoSoportado_RetornaFailure()
    {
        var result = await CrearHandler().Handle(
            new CrearProductoCommand
            {
                Tipo = (TipoProducto)999,
                Nombre = "Producto",
                Precio = 10,
                Unidades = 1
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();

        result.Error.Should()
            .Contain("Tipo de producto no soportado");
    }
}