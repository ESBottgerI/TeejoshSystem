using TeejoshSystem.Application.Common;

namespace TeejoshSystem.BlazorUI.Services;

/// <summary>
/// Antes de Fase 7, cada página repetía el mismo patrón:
///
///   try { var r = await Mediator.Send(cmd); if (!r.IsSuccess) _error = r.Error; }
///   catch (Exception ex) { _error = $"Error inesperado: {ex.Message}"; }
///
/// Este helper centraliza ese patrón en un solo lugar. No cambia el
/// contrato de Result/Result&lt;T&gt; de Application — solo evita que el
/// mismo try/catch se copie y pegue en cada página.
/// </summary>
public static class OperacionUiHelper
{
    public static async Task<string?> EjecutarAsync(Func<Task<Result>> operacion)
    {
        try
        {
            var resultado = await operacion();
            return resultado.IsSuccess ? null : resultado.Error;
        }
        catch (Exception ex)
        {
            return $"Error inesperado: {ex.Message}";
        }
    }

    public static async Task<(T? Valor, string? Error)> EjecutarAsync<T>(Func<Task<Result<T>>> operacion)
    {
        try
        {
            var resultado = await operacion();
            return resultado.IsSuccess ? (resultado.Value, null) : (default, resultado.Error);
        }
        catch (Exception ex)
        {
            return (default, $"Error inesperado: {ex.Message}");
        }
    }
}
