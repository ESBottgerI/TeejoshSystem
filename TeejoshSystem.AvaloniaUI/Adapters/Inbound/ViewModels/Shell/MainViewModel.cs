using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Styling;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;

using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Shell
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IThemePreferenceService _themeService;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private ThemeVariant _themeVariant = ThemeVariant.Default;

        [ObservableProperty]
        private ThemeOption? _selectedThemeOption;

        public List<ThemeOption> ThemeOptions { get; } = new()
        {
            new ThemeOption("Claro", ThemeVariant.Light),
            new ThemeOption("Oscuro", ThemeVariant.Dark)
        };

        public MainViewModel(IThemePreferenceService themeService)
        {
            _themeService = themeService;
        }

        public async Task InitializeAsync()
        {
            var theme = await _themeService.GetThemeAsync();
            ThemeVariant = theme;

            foreach (var option in ThemeOptions)
            {
                if (option.Value == theme)
                {
                    SelectedThemeOption = option;
                    OnPropertyChanged(nameof(SelectedThemeOption));
                    break;
                }
            }
        }

        partial void OnSelectedThemeOptionChanged(ThemeOption? value)
        {
            if (value != null && value.Value != ThemeVariant)
            {
                ThemeVariant = value.Value;
                _themeService.SaveThemeAsync(value.Value).GetAwaiter().GetResult();
            }
        }
    }

    public record ThemeOption(string Name, ThemeVariant Value);
}