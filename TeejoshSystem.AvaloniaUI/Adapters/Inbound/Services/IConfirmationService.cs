using System.Threading.Tasks;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services
{
    public interface IConfirmationService
    {
        Task<bool> Confirm(string message, string title = "Confirmar");
    }
}