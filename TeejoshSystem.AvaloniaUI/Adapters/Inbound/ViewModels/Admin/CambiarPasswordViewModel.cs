using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.CambiarPassword;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Admin
{
    public partial class CambiarPasswordViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationService _notification;
        private readonly INavigationService _navigation;
        private readonly SesionContext _sesionContext;

        private string _passwordActual = string.Empty;
        private string _passwordNuevo = string.Empty;
        private string _confirmarPassword = string.Empty;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        public CambiarPasswordViewModel(
            IMediator mediator,
            INotificationService notification,
            INavigationService navigation,
            SesionContext sesionContext)
        {
            _mediator = mediator;
            _notification = notification;
            _navigation = navigation;
            _sesionContext = sesionContext;
            PropertyChanged += (_, e) => { if (e.PropertyName == nameof(IsBusy)) CambiarPasswordCommand.NotifyCanExecuteChanged(); };
        }

        public void ActualizarPasswordActual(string password)
        {
            _passwordActual = password;
            CambiarPasswordCommand.NotifyCanExecuteChanged();
        }

        public void ActualizarPasswordNuevo(string password)
        {
            _passwordNuevo = password;
            CambiarPasswordCommand.NotifyCanExecuteChanged();
        }

        public void ActualizarConfirmarPassword(string password)
        {
            _confirmarPassword = password;
            CambiarPasswordCommand.NotifyCanExecuteChanged();
        }

        private bool PuedeCambiarPassword()
            => !string.IsNullOrWhiteSpace(_passwordActual)
            && !string.IsNullOrWhiteSpace(_passwordNuevo)
            && !string.IsNullOrWhiteSpace(_confirmarPassword)
            && !IsBusy;

        [RelayCommand(CanExecute = nameof(PuedeCambiarPassword))]
        private async Task CambiarPasswordAsync()
        {
            MensajeError = string.Empty;

            if (_passwordNuevo != _confirmarPassword)
            {
                MensajeError = "Las contraseñas nuevas no coinciden.";
                return;
            }

            IsBusy = true;
            try
            {
                var result = await _mediator.Send(new CambiarPasswordCommand(
                    _sesionContext.SesionActual!.UsuarioId,
                    _passwordActual,
                    _passwordNuevo));

                if (result.IsSuccess)
                {
                    await _notification.ShowSuccessAsync("Contraseña actualizada correctamente.");
                    await _navigation.NavigateToMenuAsync();
                }
                else
                {
                    MensajeError = result.Error ?? "Error al cambiar la contraseña.";
                }
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task VolverAsync() => _navigation.NavigateToMenuAsync();
    }
}
