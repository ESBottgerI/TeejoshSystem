using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System.Collections.ObjectModel;

using TeejoshSystem.Application.Ports.Inbound.Auditoria.Queries.ConsultarAuditLog;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Admin
{
    public partial class AuditLogViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigation;

        [ObservableProperty]
        private string? _filtroEntidad;

        [ObservableProperty]
        private string? _filtroUsuario;

        public ObservableCollection<AuditLogDto> Entradas { get; } = new();

        public AuditLogViewModel(IMediator mediator, INavigationService navigation)
        {
            _mediator = mediator;
            _navigation = navigation;
            _ = CargarAsync();
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var resultado = await _mediator.Send(new ConsultarAuditLogQuery(
                    Entidad: string.IsNullOrWhiteSpace(FiltroEntidad) ? null : FiltroEntidad,
                    Usuario: string.IsNullOrWhiteSpace(FiltroUsuario) ? null : FiltroUsuario));

                Entradas.Clear();
                foreach (var entrada in resultado)
                    Entradas.Add(entrada);
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