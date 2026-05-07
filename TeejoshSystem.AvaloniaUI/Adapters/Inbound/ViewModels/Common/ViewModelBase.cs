using CommunityToolkit.Mvvm.ComponentModel;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common
{
    /// <summary>
    /// Base class for all ViewModels, providing validation and busy state.
    /// </summary>
    public abstract partial class ViewModelBase : ObservableValidator
    {
        [ObservableProperty]
        private bool isBusy;
    }
}