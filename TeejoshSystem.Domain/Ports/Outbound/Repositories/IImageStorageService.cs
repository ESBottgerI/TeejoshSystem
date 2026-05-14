namespace TeejoshSystem.Domain.Ports.Outbound.Repositories
{
    public interface IImageStorageService
    {
        /// <summary>
        /// Guarda la imagen desde la ruta de origen y devuelve
        /// el nombre del archivo guardado internamente.
        /// Retorna null si rutaOrigen es null o vacía.
        /// </summary>
        Task<string?> SaveImageAsync(string? rutaOrigen);

        /// <summary>
        /// Devuelve la ruta absoluta a partir del nombre de archivo
        /// guardado. Retorna null si imageName es null.
        /// </summary>
        string? GetFullPath(string? imageName);
        Task<string?> SaveImageFromUrlAsync(string? url);  // NUEVO
    }
}
