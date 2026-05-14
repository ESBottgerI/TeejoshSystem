namespace TeejoshSystem.Domain.Ports.Outbound
{
    public interface ITcgCatalogoApiService
    {
        string FranquiciaNombre { get; }
        Task<List<ExpansionApiResult>> GetExpansionesAsync();
    }

    public record ExpansionApiResult(
        string Nombre,
        string? ImageUrl
    );
}