using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System;
using System.Linq;
using System.Threading.Tasks;

using TeejoshSystem.Application.Ports.Inbound.Catalogos.Commands.SincronizarCatalogos;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Catalogos
{
    public partial class SincronizarCatalogosViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationService _notification;
        private readonly INavigationService _navigation;

        [ObservableProperty]
        private string? _resultado;

        [ObservableProperty]
        private bool _hayErrores;

        public SincronizarCatalogosViewModel(
            IMediator mediator,
            INotificationService notification,
            INavigationService navigation)
        {
            _mediator = mediator;
            _notification = notification;
            _navigation = navigation;
        }

        [RelayCommand]
        private async Task SincronizarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Resultado = null;
                HayErrores = false;

                var result = await _mediator.Send(new SincronizarCatalogosCommand());

                var resumen = $"Expansiones nuevas: {result.TotalAgregadas}\n" +
                            $"Expansiones actualizadas: {result.TotalActualizadas}";

                if (result.Errores.Any())
                {
                    HayErrores = true;
                    Resultado = resumen + "\n\nErrores:\n" +
                                string.Join("\n", result.Errores);
                }
                else
                {
                    Resultado = resumen;
                    await _notification.ShowSuccessAsync(
                        "Catálogos sincronizados correctamente.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await _notification.ShowErrorAsync($"Error: {ex.Message} | {ex.InnerException?.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Volver() => _navigation.NavigateToMenu();
    }
}