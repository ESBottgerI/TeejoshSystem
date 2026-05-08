using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.RegistrarUsuario;
using TeejoshSystem.Application.Ports.Inbound.Auth.Queries.ListarUsuarios;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.DesactivarUsuario;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Admin
{
    public partial class GestionarUsuariosViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationService _notification;
        private readonly IConfirmationService _confirmation;
        private readonly INavigationService _navigation;

        public ObservableCollection<UsuarioListaDto> Usuarios { get; } = new();

        // Campos para crear usuario
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CrearUsuarioCommand))]
        private string _nuevoUsuario = string.Empty;

        private string _nuevaPassword = string.Empty;
        private string _confirmarPassword = string.Empty;

        [ObservableProperty]
        private RolUsuario _rolSeleccionado = RolUsuario.Operador;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        public RolUsuario[] Roles { get; } = (RolUsuario[])Enum.GetValues(typeof(RolUsuario));

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

            _ = CargarUsuariosAsync();
        }

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

        [RelayCommand]
        private async Task CargarUsuariosAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var lista = await _mediator.Send(new ListarUsuariosQuery());
                Usuarios.Clear();
                foreach (var u in lista)
                    Usuarios.Add(u);
            }
            finally { IsBusy = false; }
        }

        private bool PuedeCrearUsuario()
            => !string.IsNullOrWhiteSpace(NuevoUsuario)
            && !string.IsNullOrWhiteSpace(_nuevaPassword)
            && !string.IsNullOrWhiteSpace(_confirmarPassword)
            && !IsBusy;

        [RelayCommand(CanExecute = nameof(PuedeCrearUsuario))]
        private async Task CrearUsuarioAsync()
        {
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
                    new RegistrarUsuarioCommand(NuevoUsuario, _nuevaPassword, RolSeleccionado));

                if (result.IsSuccess)
                {
                    NuevoUsuario = string.Empty;
                    await _notification.ShowSuccessAsync("Usuario creado correctamente.");
                    await CargarUsuariosAsync();
                }
                else
                {
                    MensajeError = result.Error ?? "Error al crear el usuario.";
                }
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task DesactivarUsuarioAsync(UsuarioListaDto usuario)
        {
            var confirmar = await _confirmation.ConfirmAsync(
                $"¿Desactivar al usuario '{usuario.NombreUsuario}'?");
            if (!confirmar) return;

            IsBusy = true;
            try
            {
                await _mediator.Send(new DesactivarUsuarioCommand(usuario.Id));
                await _notification.ShowSuccessAsync("Usuario desactivado.");
                await CargarUsuariosAsync();
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private void Volver() => _navigation.NavigateToMenu();
    }
}