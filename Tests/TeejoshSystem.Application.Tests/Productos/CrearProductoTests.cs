using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.CrearProducto;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Productos
{
    /// <summary>
    /// Tests extendidos de CrearProductoCommandHandler.
    ///
    /// NOTA: el constructor requiere IAppLogger como tercer parámetro
    /// (agregado en 0.2.0-beta.1). Si la clase CrearProductoCommandHandlerTests
    /// existente no lo incluye, actualizar su constructor con:
    ///   _logger = Substitute.For&lt;IAppLogger&gt;()
    ///   _handler = new CrearProductoCommandHandler(_repo, _imageStorageMock, _logger)
    ///
    /// Mutantes objetivo:
    ///   - request.X is null → guard por tipo
    ///   - AddXDetalleAsync → verificado con Received(1)
    ///   - productoId → AsignarProductoId recibe el valor de AddAsync
    ///   - imageName is not null → imagen asignada vs no asignada
    ///   - Cada param del constructor de cada detalle
    /// </summary>
    public class CrearProductoExtendedTests
    {
        private readonly IProductoRepository _repo = Substitute.For<IProductoRepository>();
        private readonly IImageStorageService _imageStorage = Substitute.For<IImageStorageService>();
        private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

        private CrearProductoCommandHandler CrearHandler()
            => new(_repo, _imageStorage, _logger);

        // ── Funko — happy path ────────────────────────────────────────────────────

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
        public async Task Handle_Funko_DetalleNull_RetornaFailure()
        {
            // Mata el mutante: request.Funko is null → request.Funko is not null
            _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
            _repo.AddAsync(Arg.Any<Producto>()).Returns(1);

            var result = await CrearHandler().Handle(new CrearProductoCommand
            {
                Tipo = TipoProducto.Funko,
                Nombre = "Test",
                Precio = 10m,
                Unidades = 1,
                Funko = null  // ← guard
            }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            await _repo.DidNotReceive().AddFunkoDetalleAsync(Arg.Any<FunkoDetalle>());
        }

        // ── TCG — happy path ──────────────────────────────────────────────────────

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

        // ── Toy — happy path ──────────────────────────────────────────────────────

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

        // ── Varios — happy path ───────────────────────────────────────────────────

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

        // ── HotWheels — null guard (el happy path ya existe en tests originales) ──

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
        public async Task Handle_HotWheels_UsaIdDeAddAsyncEnAsignarProductoId()
        {
            // Mata mutante: AsignarProductoId(productoId) → AsignarProductoId(0)
            _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns((string?)null);
            _repo.AddAsync(Arg.Any<Producto>()).Returns(99); // ID específico
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
        public async Task Handle_Funko_UsaIdDeAddAsyncEnAsignarProductoId()
        {
            // Mismo patrón para Funko — productoId debe provenir de AddAsync
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

        // ── imageName is not null: imagen asignada al producto ───────────────────

        [Fact]
        public async Task Handle_ImageNameNoNull_AsignaImagePathAlProducto()
        {
            // Mata el mutante: imageName is not null → false (nunca asigna)
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
            // Mata el mutante: imageName is not null → true (siempre asigna)
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
    }
}
