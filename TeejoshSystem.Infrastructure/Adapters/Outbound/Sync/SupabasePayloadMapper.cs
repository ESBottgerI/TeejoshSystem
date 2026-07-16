using System.Collections.Generic;
using System.Text.Json;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Sync
{
    /// <summary>
    /// Mapea entidades de dominio a diccionarios con nombres de columna
    /// exactos de Supabase (snake_case) para el payload del SyncService.
    /// </summary>
    public static class SupabasePayloadMapper
    {
        public static string ToProductoJson(Producto p) =>
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["Id"]         = p.Id,
                ["type"]       = p.Tipo.ToString(),
                ["name"]       = p.Nombre.Value,
                ["price"]      = p.Precio.Value,
                ["units"]      = p.Stock.Value,
                ["image_path"] = p.ImagePath
            });

        public static string ToHotWheelsJson(HotWheelsDetalle d) =>
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["product_id"]  = d.ProductoId,
                ["model"]       = d.Modelo,
                ["year"]        = d.Anio,
                ["serie"]       = d.Serie,
                ["category_id"] = d.CategoriaId
            });

        public static string ToFunkoJson(FunkoDetalle d) =>
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["product_id"]         = d.ProductoId,
                ["box_number"]         = d.NumeroCaja,
                ["license"]            = d.Licencia,
                ["subtype_id"]         = d.SubtipoId,
                ["special_feature_id"] = d.CaracteristicaEspecialId
            });

        public static string ToTcgJson(TcgDetalle d) =>
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["product_id"]   = d.ProductoId,
                ["pack_id"]      = d.PackId,
                ["expansion_id"] = d.ExpansionId
            });

        public static string ToToyJson(ToyDetalle d) =>
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["product_id"]  = d.ProductoId,
                ["min_years"]   = d.EdadMinima,
                ["min_players"] = d.JugadoresMin,
                ["max_players"] = d.JugadoresMax,
                ["board_game"]  = d.EsJuegoDeMesa
            });

        public static string ToVariosJson(VariosDetalle d) =>
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["product_id"] = d.ProductoId,
                ["brand"]      = d.Marca,
                ["height"]     = d.Alto,
                ["width"]      = d.Ancho,
                ["length"]     = d.Largo,
                ["material"]   = d.Material,
                ["ilustration"] = d.TieneIlustracion
            });
    }
}