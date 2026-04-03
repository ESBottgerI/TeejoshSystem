

namespace TeejoshInventario.WPF.Adapters.Inbound.Services
{
    public interface IConfirmationService
    {
        bool Confirm(string message, string title = "Confirmacion");
    }
}
