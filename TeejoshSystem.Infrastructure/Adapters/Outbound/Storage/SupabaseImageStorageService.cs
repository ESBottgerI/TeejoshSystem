using System.Net.Http.Headers;
using Svg.Skia;
using SkiaSharp;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Storage
{
    /// <summary>
    /// Implementación de IImageStorageService para Supabase Storage.
    /// Usa HttpClient raw (REST API) en lugar del SDK oficial de Supabase para C#,
    /// que introduce conflictos de dependencias con Avalonia (System.Reactive, etc.).
    ///
    /// Endpoint base: https://{project-ref}.supabase.co/storage/v1/object/{bucket}/{path}
    /// Autenticación: service_role key en header Authorization: Bearer {key}
    ///
    /// GetFullPath devuelve la URL pública del objeto en Supabase Storage.
    /// El bucket debe estar configurado como público en el dashboard de Supabase.
    /// </summary>
    public class SupabaseImageStorageService : IImageStorageService
    {
        private readonly HttpClient _http;
        private readonly string _bucketUrl;   // https://{ref}.supabase.co/storage/v1/object/{bucket}
        private readonly string _publicUrl;   // https://{ref}.supabase.co/storage/v1/object/public/{bucket}

        // Inyectado desde IConfiguration — ver InfrastructureServiceRegistration
        public SupabaseImageStorageService(
            string supabaseUrl,
            string supabaseServiceKey,
            string bucketName)
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", supabaseServiceKey);
            _http.DefaultRequestHeaders.Add("apikey", supabaseServiceKey);

            // Normalizar URL base (quitar trailing slash)
            var baseUrl = supabaseUrl.TrimEnd('/');
            _bucketUrl = $"{baseUrl}/storage/v1/object/{bucketName}";
            _publicUrl = $"{baseUrl}/storage/v1/object/public/{bucketName}";
        }

        /// <summary>
        /// Copia un archivo local a Supabase Storage y retorna el nombre del objeto (GUID + ext).
        /// </summary>
        public async Task<string?> SaveImageAsync(string? rutaOrigen)
        {
            if (string.IsNullOrWhiteSpace(rutaOrigen))
                return null;

            if (!File.Exists(rutaOrigen))
                return null;

            var extension = Path.GetExtension(rutaOrigen).ToLowerInvariant();
            var objectName = $"{Guid.NewGuid()}{extension}";

            var bytes = await File.ReadAllBytesAsync(rutaOrigen);
            return await UploadBytesAsync(bytes, objectName, GetContentType(extension));
        }

        /// <summary>
        /// Descarga una imagen desde una URL externa y la sube a Supabase Storage.
        /// Los SVG se convierten a PNG antes de subir (compatibilidad con Avalonia Image).
        /// </summary>
        public async Task<string?> SaveImageFromUrlAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                using var downloadClient = new HttpClient();
                var bytes = await downloadClient.GetByteArrayAsync(url);

                var extension = Path.GetExtension(new Uri(url).AbsolutePath)
                    .ToLowerInvariant();

                if (extension == ".svg")
                    return await ConvertirSvgYSubirAsync(bytes);

                if (string.IsNullOrWhiteSpace(extension))
                    extension = ".png";

                var objectName = $"{Guid.NewGuid()}{extension}";
                return await UploadBytesAsync(bytes, objectName, GetContentType(extension));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"SupabaseImageStorageService.SaveImageFromUrlAsync error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Devuelve la URL pública del objeto en Supabase Storage.
        /// Retorna null si imageName es null o vacío.
        /// </summary>
        public string? GetFullPath(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return null;

            // Si ya es una URL completa, devolverla tal cual
            if (imageName.StartsWith("http://") || imageName.StartsWith("https://"))
                return imageName;

            return $"{_publicUrl}/{imageName}";
        }

        // ── Helpers privados ──────────────────────────────────────────────────

        private async Task<string?> UploadBytesAsync(
            byte[] bytes,
            string objectName,
            string contentType)
        {
            try
            {
                using var content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                var response = await _http.PostAsync(
                    $"{_bucketUrl}/{objectName}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine(
                        $"Supabase Storage upload failed [{response.StatusCode}]: {body}");
                    return null;
                }

                return objectName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"SupabaseImageStorageService.UploadBytesAsync error: {ex.Message}");
                return null;
            }
        }

        private async Task<string?> ConvertirSvgYSubirAsync(byte[] svgBytes)
        {
            try
            {
                var pngBytes = await Task.Run(() =>
                {
                    var svg = new SKSvg();
                    using var stream = new MemoryStream(svgBytes);
                    svg.Load(stream);

                    if (svg.Picture is null) return null;

                    const int size = 256;
                    var imageInfo = new SKImageInfo(size, size,
                        SKColorType.Rgba8888, SKAlphaType.Premul);

                    using var surface = SKSurface.Create(imageInfo);
                    var canvas = surface.Canvas;
                    canvas.Clear(SKColors.Transparent);

                    var bounds = svg.Picture.CullRect;
                    var scale = Math.Min(size / bounds.Width, size / bounds.Height);
                    canvas.Scale(scale);
                    canvas.DrawPicture(svg.Picture);

                    using var image = surface.Snapshot();
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    return data.ToArray();
                });

                if (pngBytes is null) return null;

                var objectName = $"{Guid.NewGuid()}.png";
                return await UploadBytesAsync(pngBytes, objectName, "image/png");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"SupabaseImageStorageService.ConvertirSvgYSubirAsync error: {ex.Message}");
                return null;
            }
        }

        private static string GetContentType(string extension) => extension switch
        {
            ".png"  => "image/png",
            ".jpg"  => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif"  => "image/gif",
            ".webp" => "image/webp",
            _       => "application/octet-stream"
        };
    }
}