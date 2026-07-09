using Microsoft.AspNetCore.Components.Forms;

namespace TeejoshSystem.BlazorUI.Services;

/// <summary>
/// CrearProductoCommand/ActualizarProductoCommand.ImagePath espera una ruta
/// de archivo LEGIBLE DESDE EL SERVIDOR — así lo diseñó IImageStorageService
/// (Infrastructure), pensado originalmente para Avalonia, donde el usuario
/// elige un archivo de su propio disco y la app tiene acceso directo a esa
/// ruta. En Blazor Server el navegador solo entrega bytes (IBrowserFile),
/// nunca una ruta real del lado del servidor.
///
/// Este helper "materializa" esos bytes como un archivo temporal en el
/// servidor y devuelve esa ruta, para que encaje en el Command sin tocar
/// Application/Infrastructure. El caller es responsable de borrar el
/// temporal después de que Mediator.Send() retorne (éxito o error) —
/// ver BorrarSiExiste.
/// </summary>
public static class ImagenTemporalHelper
{
    private const long TamanioMaximoBytes = 5 * 1024 * 1024; // 5 MB

    public static async Task<string> GuardarComoTemporalAsync(IBrowserFile archivo, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(archivo.Name);
        var rutaTemporal = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");

        await using var streamDestino = File.Create(rutaTemporal);
        await using var streamOrigen = archivo.OpenReadStream(TamanioMaximoBytes, ct);
        await streamOrigen.CopyToAsync(streamDestino, ct);

        return rutaTemporal;
    }

    public static void BorrarSiExiste(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta))
        {
            return;
        }

        try
        {
            File.Delete(ruta);
        }
        catch
        {
            // Best-effort: un temporal huérfano no es crítico, no debe
            // interrumpir el flujo de creación/edición del producto.
        }
    }
}