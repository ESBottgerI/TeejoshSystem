using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Menu;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Shell
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private object? _currentView;

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            // Evitamos el ciclo usando el constructor vacío de MenuPrincipalViewModel
            CurrentView = new MenuPrincipalViewModel();
        }

        // public MainViewModel(IServiceProvider serviceProvider)
        // {
        //     _serviceProvider = serviceProvider;
        //     // Resolver el menú principal desde DI
        //     CurrentView = _serviceProvider.GetRequiredService<MenuPrincipalViewModel>();
        // }

        // partial void OnCurrentViewChanged(object? value)
        // {
        //     if (value is ILoadable loadable)
        //         loadable.OnLoaded();
        // }
    }
}