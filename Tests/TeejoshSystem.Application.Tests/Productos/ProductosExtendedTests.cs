using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.ActualizarProducto;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.CrearProducto;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Productos;

// ═══════════════════════════════════════════════════════════════════════════
// ActualizarProductoCommandHandler — survivors
// Constructor: (IProductoRepository, IImageStorageService, IAppLogger)
// ═══════════════════════════════════════════════════════════════════════════

public class ActualizarProductoExtendedTests
{
    private readonly IProductoRepository _repo = Substitute.For<IProductoRepository>();
    private readonly IImageStorageService _imageStorage = Substitute.For<IImageStorageService>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private ActualizarProductoCommandHandler CrearHandler()
        => new(_repo, _imageStorage, _logger);

    private static Producto ProductoExistente()
        => new(TipoProducto.HotWheels,
               new NombreProducto("Camaro"),
               new Precio(25m),
               new Unidades(5));

    // ── Happy path sin imagen ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductoExisteSinImagen_RetornaSuccessYLlamaUpdateAsync()
    {
        _repo.GetByIdAsync(1).Returns(ProductoExistente());
        _repo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Camaro 1969", 30m, 10, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(Arg.Any<Producto>());
    }

    // ── ImagePath is not null — par ambas ramas ───────────────────────────────

    [Fact]
    public async Task Handle_ImagePathNull_NoLlamaSaveImageAsync()
    {
        // Mata mutante: ImagePath is not null → true (siempre guarda)
        _repo.GetByIdAsync(1).Returns(ProductoExistente());
        _repo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Camaro", 25m, 5, null),
            CancellationToken.None);

        await _imageStorage.DidNotReceive().SaveImageAsync(Arg.Any<string?>());
    }

    [Fact]
    public async Task Handle_ImagePathNoNull_LlamaSaveImageAsyncYAsignaPath()
    {
        // Mata mutante: ImagePath is not null → false (nunca guarda)
        _repo.GetByIdAsync(1).Returns(ProductoExistente());
        _imageStorage.SaveImageAsync("origen/camaro.jpg").Returns("guardada/camaro_001.jpg");
        _repo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Camaro", 30m, 5, "origen/camaro.jpg"),
            CancellationToken.None);

        await _imageStorage.Received(1).SaveImageAsync("origen/camaro.jpg");
        await _repo.Received(1).UpdateAsync(
            Arg.Is<Producto>(p => p.ImagePath == "guardada/camaro_001.jpg"));
    }

    // ── producto is null ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductoNoExiste_RetornaFailureSinLlamarUpdate()
    {
        _repo.GetByIdAsync(99).Returns((Producto?)null);

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(99, "Test", 10m, 1, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("El producto no existe.");
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Producto>());
    }

    // ── ActualizarDatos: parámetros correctos ─────────────────────────────────

    [Fact]
    public async Task Handle_ActualizarDatos_PasaParametrosCorrectos()
    {
        // Mata mutantes: Nombre, Precio, Unidades pasados con valores incorrectos
        _repo.GetByIdAsync(1).Returns(ProductoExistente());

        Producto? capturado = null;
        _repo.UpdateAsync(Arg.Do<Producto>(p => capturado = p)).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Nombre Nuevo", 99.99m, 42, null),
            CancellationToken.None);

        capturado!.Nombre.Value.Should().Be("Nombre Nuevo");
        capturado.Precio.Value.Should().Be(99.99m);
        capturado.Stock.Value.Should().Be(42);
    }

    // ── catch blocks ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NombreInvalido_RetornaFailurePorArgumentException()
    {
        _repo.GetByIdAsync(1).Returns(ProductoExistente());

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "", 25m, 5, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Producto>());
    }

    [Fact]
    public async Task Handle_ExcepcionGenerica_RetornaFailureConMensajeFijo()
    {
        _repo.When(x => x.GetByIdAsync(Arg.Any<int>()))
             .Throw(new Exception("Error de red"));

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Test", 25m, 5, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("guardar el producto");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// CrearProductoCommandHandler — survivors
// Constructor: (IProductoRepository, IImageStorageService, IAppLogger)
// ═══════════════════════════════════════════════════════════════════════════

public class CrearProductoExtendedTests
{
    private readonly IProductoRepository _repo = Substitute.For<IProductoRepository>();
    private readonly IImageStorageService _imageStorage = Substitute.For<IImageStorageService>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private CrearProductoCommandHandler CrearHandler()
        => new(_repo, _imageStorage, _logger);

    // ── Funko — happy path y null guard ──────────────────────────────────────

    [Fact]
    public async Task Handle_Funko_Valido_RetornaSuccessYLlamaAddFunkoDetalleAsync()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repo.AddAsync(Arg.Any<Producto>()).Returns(1);
        _repo.AddFunkoDetalleAsync(Arg.Any<FunkoDetalle>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Funko,
            Nombre = "Pikachu 25°",
            Precio = 15m,
            Unidades = 2,
            Funko = new CrearFunkoDetalleDto(500, "Pokémon", 1, null)
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddFunkoDetalleAsync(Arg.Any<FunkoDetalle>());
    }

    [Fact]
    public async Task Handle_Funko_DetalleNull_RetornaFailureSinAgregarDetalle()
    {
        // Mata mutante: request.Funko is null → false
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repo.AddAsync(Arg.Any<Producto>()).Returns(1);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Funko,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            Funko = null
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repo.DidNotReceive().AddFunkoDetalleAsync(Arg.Any<FunkoDetalle>());
    }

    // ── TCG ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Tcg_Valido_RetornaSuccessYLlamaAddTcgDetalleAsync()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repo.AddAsync(Arg.Any<Producto>()).Returns(2);
        _repo.AddTcgDetalleAsync(Arg.Any<TcgDetalle>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Tcg,
            Nombre = "Booster Base Set",
            Precio = 8m,
            Unidades = 10,
            Tcg = new CrearTcgDetalleDto(1, 1)
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddTcgDetalleAsync(Arg.Any<TcgDetalle>());
    }

    [Fact]
    public async Task Handle_Tcg_DetalleNull_RetornaFailure()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repo.AddAsync(Arg.Any<Producto>()).Returns(2);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Tcg,
            Nombre = "Test",
            Precio = 8m,
            Unidades = 1,
            Tcg = null
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repo.DidNotReceive().AddTcgDetalleAsync(Arg.Any<TcgDetalle>());
    }

    // ── Toy ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Toy_Valido_RetornaSuccessYLlamaAddToyDetalleAsync()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repo.AddAsync(Arg.Any<Producto>()).Returns(3);
        _repo.AddToyDetalleAsync(Arg.Any<ToyDetalle>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Toy,
            Nombre = "Monopoly Clásico",
            Precio = 45m,
            Unidades = 5,
            Toy = new CrearToyDetalleDto(8, 2, 6, true)
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddToyDetalleAsync(Arg.Any<ToyDetalle>());
    }

    [Fact]
    public async Task Handle_Toy_DetalleNull_RetornaFailure()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repo.AddAsync(Arg.Any<Producto>()).Returns(3);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Toy,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            Toy = null
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repo.DidNotReceive().AddToyDetalleAsync(Arg.Any<ToyDetalle>());
    }

    // ── Varios ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Varios_Valido_RetornaSuccessYLlamaAddVariosDetalleAsync()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repo.AddAsync(Arg.Any<Producto>()).Returns(4);
        _repo.AddVariosDetalleAsync(Arg.Any<VariosDetalle>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Varios,
            Nombre = "Figura Bandai",
            Precio = 35m,
            Unidades = 3,
            Varios = new CrearVariosDetalleDto("Bandai", 15m, 10m, null, "PVC", false)
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddVariosDetalleAsync(Arg.Any<VariosDetalle>());
    }

    [Fact]
    public async Task Handle_Varios_DetalleNull_RetornaFailure()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repo.AddAsync(Arg.Any<Producto>()).Returns(4);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.Varios,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            Varios = null
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repo.DidNotReceive().AddVariosDetalleAsync(Arg.Any<VariosDetalle>());
    }

    // ── HotWheels null guard ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_HotWheels_DetalleNull_RetornaFailure()
    {
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repo.AddAsync(Arg.Any<Producto>()).Returns(5);

        var result = await CrearHandler().Handle(new CrearProductoCommand
        {
            Tipo = TipoProducto.HotWheels,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            HotWheels = null
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repo.DidNotReceive().AddHotWheelsDetalleAsync(Arg.Any<HotWheelsDetalle>());
    }

    // ── productoId de AddAsync pasado a AsignarProductoId ────────────────────

    [Fact]
    public async Task Handle_HotWheels_AsignarProductoId_UsaElIdDeAddAsync()
    {
        // Mata mutante: AsignarProductoId(productoId) → AsignarProductoId(0)
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
        _repo.AddAsync(Arg.Any<Producto>()).Returns(99);

        HotWheelsDetalle? capturado = null;
        _repo.AddHotWheelsDetalleAsync(Arg.Do<HotWheelsDetalle>(d => capturado = d))
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
        _repo.AddAsync(Arg.Any<Producto>()).Returns(77);

        FunkoDetalle? capturado = null;
        _repo.AddFunkoDetalleAsync(Arg.Do<FunkoDetalle>(d => capturado = d))
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
        _repo.AddAsync(Arg.Do<Producto>(p => capturado = p)).Returns(1);
        _repo.AddHotWheelsDetalleAsync(Arg.Any<HotWheelsDetalle>()).Returns(Task.CompletedTask);

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
        _repo.AddAsync(Arg.Do<Producto>(p => capturado = p)).Returns(1);
        _repo.AddHotWheelsDetalleAsync(Arg.Any<HotWheelsDetalle>()).Returns(Task.CompletedTask);

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
}