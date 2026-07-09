using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.BlazorUI.Services;

/// <summary>
/// ProductoDto (usado por ObtenerProductosQuery) ya trae TipoDescripcion
/// calculado desde el backend. ProductoBusquedaDto (usado por
/// BuscarProductosQuery) NO lo trae — solo el enum crudo. Este helper evita
/// tener el texto de cada tipo duplicado/hardcodeado en cada página que
/// consuma BuscarProductosQuery.
/// </summary>
public static class TipoProductoDisplay
{
    public static string ToDisplayName(TipoProducto tipo) => tipo switch
    {
        TipoProducto.Funko => "Funko",
        TipoProducto.HotWheels => "Hot Wheels",
        TipoProducto.Tcg => "TCG",
        TipoProducto.Toy => "Juguete",
        TipoProducto.Varios => "Varios",
        _ => tipo.ToString()
    };
}