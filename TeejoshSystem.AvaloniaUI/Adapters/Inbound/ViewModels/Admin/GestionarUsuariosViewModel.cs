using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.DesactivarUsuario;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.RegistrarUsuario;
using TeejoshSystem.Application.Ports.Inbound.Auth.Queries.ListarUsuarios;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;
using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Admin;

public partial class GestionarUsuariosViewModel : ViewModelBase, ILoadable
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notification;
    private readonly IConfirmationService _confirmation;
    private readonly INavigationService _navigation;

    public ObservableCollection<UsuarioListaDto> Usuarios { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CrearUsuarioCommand))]
    private string _nuevoUsuario = string.Empty;

    private string _nuevaPassword = string.Empty;
    private string _confirmarPassword = string.Empty;

    [ObservableProperty]
    private RolUsuario _rolSeleccionado = RolUsuario.Operador;

    [ObservableProperty]
    private string _mensajeError = string.Empty;

    public RolUsuario[] Roles { get; } = Enum.GetValues<RolUsuario>();

    public GestionarUsuariosViewModel(
        IMediator mediator,
        INotificationService notification,
        IConfirmationService confirmation,
        INavigationService navigation)
    {
        _mediator = mediator;
        _notification = notification;
        _confirmation = confirmation;
        _navigation = navigation;

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(IsBusy)) return;
            CrearUsuarioCommand.NotifyCanExecuteChanged();
            DesactivarUsuarioCommand.NotifyCanExecuteChanged();
            CargarUsuariosCommand.NotifyCanExecuteChanged();
        };
    }

    public Task LoadAsync(CancellationToken cancellationToken = default) =>
        CargarUsuariosAsync(cancellationToken);

    public void ActualizarNuevaPassword(string password)
    {
        _nuevaPassword = password;
        CrearUsuarioCommand.NotifyCanExecuteChanged();
    }

    public void ActualizarConfirmarPassword(string password)
    {
        _confirmarPassword = password;
        CrearUsuarioCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(PuedeCargarUsuarios))]
    private async Task CargarUsuariosAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) return;

        IsBusy = true;
        MensajeError = string.Empty;
        try
        {
            await CargarUsuariosCoreAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MensajeError = "No se pudieron cargar los usuarios.";
            await _notification.ShowErrorAsync(MensajeError + " " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CargarUsuariosCoreAsync(CancellationToken cancellationToken = default)
    {
        var lista = await _mediator.Send(new ListarUsuariosQuery(), cancellationToken);
        Usuarios.Clear();
        foreach (var usuario in lista)
            Usuarios.Add(usuario);
    }

    private bool PuedeCargarUsuarios() => !IsBusy;

    private bool PuedeCrearUsuario() =>
        !string.IsNullOrWhiteSpace(NuevoUsuario) &&
        !string.IsNullOrWhiteSpace(_nuevaPassword) &&
        !string.IsNullOrWhiteSpace(_confirmarPassword) &&
        string.Equals(_nuevaPassword, _confirmarPassword, StringComparison.Ordinal) &&
        !IsBusy;

    [RelayCommand(CanExecute = nameof(PuedeCrearUsuario))]
    private async Task CrearUsuarioAsync()
    {
        if (IsBusy) return;

        MensajeError = string.Empty;
        if (_nuevaPassword != _confirmarPassword)
        {
            MensajeError = "Las contraseñas no coinciden.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(
                new RegistrarUsuarioCommand(NuevoUsuario.Trim(), _nuevaPassword, RolSeleccionado));

            if (!result.IsSuccess)
            {
                MensajeError = result.Error ?? "Error al crear el usuario.";
                return;
            }

            NuevoUsuario = string.Empty;
            _nuevaPassword = string.Empty;
            _confirmarPassword = string.Empty;
            await _notification.ShowSuccessAsync("Usuario creado correctamente.");
            await CargarUsuariosCoreAsync();
        }
        catch (Exception ex)
        {
            MensajeError = "No se pudo crear el usuario.";
            await _notification.ShowErrorAsync(MensajeError + " " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool PuedeDesactivarUsuario(UsuarioListaDto? usuario) => usuario is not null && !IsBusy;

    [RelayCommand(CanExecute = nameof(PuedeDesactivarUsuario))]
    private async Task DesactivarUsuarioAsync(UsuarioListaDto? usuario)
    {
        if (usuario is null || IsBusy) return;

        var confirmar = await _confirmation.ConfirmAsync(
            $"¿Desactivar al usuario '{usuario.NombreUsuario}'?");
        if (!confirmar || IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new DesactivarUsuarioCommand(usuario.Id));
            if (!result.IsSuccess)
            {
                MensajeError = result.Error ?? "No se pudo desactivar el usuario.";
                await _notification.ShowErrorAsync(MensajeError);
                return;
            }

            await _notification.ShowSuccessAsync("Usuario desactivado.");
            await CargarUsuariosCoreAsync();
        }
        catch (Exception ex)
        {
            MensajeError = "No se pudo desactivar el usuario.";
            await _notification.ShowErrorAsync(MensajeError + " " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task VolverAsync() => _navigation.NavigateToMenuAsync();
}