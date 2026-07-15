using MediatR;
using NSubstitute;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.RegistrarUsuario;
using TeejoshSystem.Application.Ports.Inbound.Auth.Queries.ListarUsuarios;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerCatalogos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.CrearProducto;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Admin;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;
using TeejoshSystem.Domain.Ports.Outbound;
using Xunit;

namespace TeejoshSystem.AvaloniaUI.Tests;

public sealed class AsyncViewModelTests
{
    [Fact]
    public async Task Navigation_DoesNotPublishDestinationUntilLoadCompletes()
    {
        var navigation = new NavigationService();
        var load = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var viewModel = new PendingLoadable(load.Task);
        object? published = null;
        navigation.Configure(value => published = value, () => { });

        var navigating = navigation.NavigateToAsync(viewModel);
        published.Should().BeNull();
        navigating.IsCompleted.Should().BeFalse();

        load.SetResult();
        await navigating;
        published.Should().BeSameAs(viewModel);
    }

    [Fact]
    public async Task CrearProducto_LoadErrorRestoresBusyAndNotifies()
    {
        var (vm, mediator, notifications, _, _) = CreateProductViewModel();
        mediator.Send(Arg.Any<ObtenerCatalogosQuery>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<CatalogosDto>(new InvalidOperationException("catálogos caídos")));

        await vm.LoadAsync();

        vm.IsBusy.Should().BeFalse();
        vm.CatalogosCargados.Should().BeFalse();
        vm.GuardarCommand.CanExecute(null).Should().BeFalse();
        await notifications.Received(1).ShowErrorAsync(Arg.Is<string>(text => text.Contains("catálogos caídos")));
    }

    [Fact]
    public async Task CrearProducto_DoubleSubmitSendsOnlyOnceAndDisablesCanExecuteWhileBusy()
    {
        var (vm, mediator, _, confirmation, navigation) = CreateProductViewModel();
        mediator.Send(Arg.Any<ObtenerCatalogosQuery>(), Arg.Any<CancellationToken>())
            .Returns(EmptyCatalogs());
        await vm.LoadAsync();
        MakeValid(vm);
        confirmation.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        var pending = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        mediator.Send(Arg.Any<CrearProductoCommand>(), Arg.Any<CancellationToken>()).Returns(pending.Task);

        var first = vm.GuardarCommand.ExecuteAsync(null);
        vm.IsBusy.Should().BeTrue();
        vm.GuardarCommand.CanExecute(null).Should().BeFalse();
        var second = vm.GuardarCommand.ExecuteAsync(null);

        pending.SetResult(Result.Success());
        await Task.WhenAll(first, second);

        await mediator.Received(1).Send(Arg.Any<CrearProductoCommand>(), Arg.Any<CancellationToken>());
        await navigation.Received(1).NavigateToMenuAsync(Arg.Any<CancellationToken>());
        vm.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task GestionarUsuarios_LoadIsExplicitBusyAndRestoresOnError()
    {
        var (vm, mediator, notifications, _, _) = CreateUsersViewModel();
        var pending = new TaskCompletionSource<IEnumerable<UsuarioListaDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        mediator.Send(Arg.Any<ListarUsuariosQuery>(), Arg.Any<CancellationToken>()).Returns(pending.Task);

        var load = vm.LoadAsync();
        vm.IsBusy.Should().BeTrue();
        pending.SetException(new InvalidOperationException("sin conexión"));
        await load;

        vm.IsBusy.Should().BeFalse();
        await notifications.Received(1).ShowErrorAsync(Arg.Is<string>(text => text.Contains("sin conexión")));
    }

    [Fact]
    public async Task GestionarUsuarios_CanExecuteValidatesPasswordsAndPreventsDoubleSubmit()
    {
        var (vm, mediator, _, _, _) = CreateUsersViewModel();
        vm.NuevoUsuario = "nuevo";
        vm.ActualizarNuevaPassword("clave-123");
        vm.ActualizarConfirmarPassword("otra");
        vm.CrearUsuarioCommand.CanExecute(null).Should().BeFalse();
        vm.ActualizarConfirmarPassword("clave-123");
        vm.CrearUsuarioCommand.CanExecute(null).Should().BeTrue();

        var pending = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        mediator.Send(Arg.Any<RegistrarUsuarioCommand>(), Arg.Any<CancellationToken>()).Returns(pending.Task);
        mediator.Send(Arg.Any<ListarUsuariosQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<UsuarioListaDto>());

        var first = vm.CrearUsuarioCommand.ExecuteAsync(null);
        vm.IsBusy.Should().BeTrue();
        vm.CrearUsuarioCommand.CanExecute(null).Should().BeFalse();
        var second = vm.CrearUsuarioCommand.ExecuteAsync(null);
        pending.SetResult(Result.Success());
        await Task.WhenAll(first, second);

        await mediator.Received(1).Send(Arg.Any<RegistrarUsuarioCommand>(), Arg.Any<CancellationToken>());
        vm.IsBusy.Should().BeFalse();
    }


    [Fact]
    public async Task Inventario_StockStartsPrivateAndSupportsRowAndGlobalCommands()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos.BuscarProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos.ProductoBusquedaDto>
            {
                new() { Id = 1, Nombre = "Uno", Precio = 1m, Unidades = 4, Tipo = TeejoshSystem.Domain.Enums.TipoProducto.HotWheels, DetalleResumen = "A" },
                new() { Id = 2, Nombre = "Dos", Precio = 2m, Unidades = 0, Tipo = TeejoshSystem.Domain.Enums.TipoProducto.Funko, DetalleResumen = "B" }
            });
        var vm = new InventarioViewModel(mediator, () => Task.CompletedTask);
        await vm.LoadAsync();

        vm.Productos[0].StockTexto.Should().Be("Disponible");
        vm.Productos[1].StockTexto.Should().Be("Sin stock");
        vm.AlternarStockFilaCommand.CanExecute(vm.Productos[0]).Should().BeTrue();
        vm.AlternarStockFilaCommand.Execute(vm.Productos[0]);
        vm.Productos[0].StockTexto.Should().Be("4");
        vm.Productos[1].StockTexto.Should().Be("Sin stock");

        vm.AlternarStockGlobalCommand.Execute(null);
        vm.Productos.Select(item => item.StockTexto).Should().Equal("4", "0");
        vm.AlternarStockGlobalCommand.Execute(null);
        vm.Productos.Select(item => item.StockTexto).Should().Equal("Disponible", "Sin stock");
    }

    private static (CrearProductoViewModel Vm, IMediator Mediator, INotificationService Notifications,
        IConfirmationService Confirmation, INavigationService Navigation) CreateProductViewModel()
    {
        var mediator = Substitute.For<IMediator>();
        var notifications = Substitute.For<INotificationService>();
        var confirmation = Substitute.For<IConfirmationService>();
        var navigation = Substitute.For<INavigationService>();
        var images = Substitute.For<IImageStorageService>();
        return (new CrearProductoViewModel(mediator, notifications, confirmation, navigation, images),
            mediator, notifications, confirmation, navigation);
    }

    private static (GestionarUsuariosViewModel Vm, IMediator Mediator, INotificationService Notifications,
        IConfirmationService Confirmation, INavigationService Navigation) CreateUsersViewModel()
    {
        var mediator = Substitute.For<IMediator>();
        var notifications = Substitute.For<INotificationService>();
        var confirmation = Substitute.For<IConfirmationService>();
        var navigation = Substitute.For<INavigationService>();
        return (new GestionarUsuariosViewModel(mediator, notifications, confirmation, navigation),
            mediator, notifications, confirmation, navigation);
    }

    private static CatalogosDto EmptyCatalogs() => new()
    {
        CategoriasHotWheels = new List<CatalogoItemDto>(),
        SubtiposFunko = new List<CatalogoItemDto>(),
        CaracteristicasFunko = new List<CatalogoItemDto>(),
        FranquiciasTcg = new List<CatalogoItemDto>()
    };

    private static void MakeValid(CrearProductoViewModel vm)
    {
        vm.Nombre = "Mustang";
        vm.HwModelo = "GT";
        vm.HwSerie = "Race";
        vm.HwCategoriaSeleccionada = new CatalogoItemDto { Id = 1, Nombre = "Premium" };
    }

    private sealed class PendingLoadable(Task task) : ILoadable
    {
        public Task LoadAsync(CancellationToken cancellationToken = default) => task;
    }
}