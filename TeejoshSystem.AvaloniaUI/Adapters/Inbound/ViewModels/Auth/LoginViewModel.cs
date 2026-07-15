using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.AutenticarUsuario;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Auth
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly SesionContext _sesionContext;

        // Callback configurado desde App.axaml.cs.
        // LoginViewModel no conoce NavigationService ni MainViewModel —
        // el acoplamiento de navegación vive en el punto de composición.
        public Func<Task>? OnLoginExitoso { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(IniciarSesionCommand))]
        private string _nombreUsuario = string.Empty;

        // La contraseña NO es ObservableProperty.
        // Un campo privado evita que el valor en texto plano sea serializado,
        // logueado por herramientas de diagnóstico, o expuesto via reflection.
        private string _password = string.Empty;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        public LoginViewModel(IMediator mediator, SesionContext sesionContext)
        {
            _mediator = mediator;
            _sesionContext = sesionContext;
        }

        /// <summary>
        /// Llamado desde code-behind cuando cambia el TextBox de contraseña.
        /// Avalonia no soporta binding directo a contraseñas.
        /// </summary>
        public void ActualizarPassword(string password)
        {
            _password = password;
            IniciarSesionCommand.NotifyCanExecuteChanged();
        }

        private bool PuedeIniciarSesion()
            => !string.IsNullOrWhiteSpace(NombreUsuario)
            && !string.IsNullOrWhiteSpace(_password)
            && !IsBusy;

        [RelayCommand(CanExecute = nameof(PuedeIniciarSesion))]
        private async Task IniciarSesionAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            MensajeError = string.Empty;

            try
            {
                var result = await _mediator.Send(
                    new AutenticarUsuarioCommand(NombreUsuario, _password));

                if (result.IsSuccess)
                {
                    _sesionContext.IniciarSesion(result.Value);
                    if (OnLoginExitoso is not null)
                        await OnLoginExitoso();
                }
                else
                {
                    MensajeError = result.Error ?? "Error desconocido.";
                }
            }
            finally
            {
                IsBusy = false;
                IniciarSesionCommand.NotifyCanExecuteChanged();
            }
        }
    }
}