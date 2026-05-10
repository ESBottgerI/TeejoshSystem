using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Storage
{
    public class LocalImageStorageService : IImageStorageService
    {
        private readonly string _imagesFolder;
        private static readonly HttpClient _http = new();

        public LocalImageStorageService()
        {
            _imagesFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TeejoshSystem",
                "images");

            Directory.CreateDirectory(_imagesFolder);
        }

        public async Task<string?> SaveImageAsync(string? rutaOrigen)
        {
            if (string.IsNullOrWhiteSpace(rutaOrigen))
                return null;

            if (!File.Exists(rutaOrigen))
                return null;

            var extension = Path.GetExtension(rutaOrigen).ToLowerInvariant();
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaDestino = Path.Combine(_imagesFolder, nombreArchivo);

            await Task.Run(() => File.Copy(rutaOrigen, rutaDestino, overwrite: true));

            return nombreArchivo;
        }

        public string? GetFullPath(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return null;

            var fullPath = Path.Combine(_imagesFolder, imageName);
            return File.Exists(fullPath) ? fullPath : null;
        }

        // NUEVO
        public async Task<string?> SaveImageFromUrlAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                var bytes = await _http.GetByteArrayAsync(url);
                var extension = Path.GetExtension(
                    new Uri(url).AbsolutePath).ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(extension))
                    extension = ".png";

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaDestino = Path.Combine(_imagesFolder, nombreArchivo);

                await File.WriteAllBytesAsync(rutaDestino, bytes);
                return nombreArchivo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"SaveImageFromUrlAsync error: {ex.Message}");
                return null;
            }
        }
    }
}