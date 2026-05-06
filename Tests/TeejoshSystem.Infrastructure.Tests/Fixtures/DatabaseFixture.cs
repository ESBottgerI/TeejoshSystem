using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Infrastructure.Tests.Fixtures;

/// <summary>
/// Fixture compartida por clase de test (IClassFixture).
/// Crea un archivo SQLite temporal por instancia, aplica las migraciones
/// reales y lo elimina al finalizar.
///
/// Por qué archivo y no :memory::
/// SQLite :memory: no soporta múltiples conexiones en el mismo proceso,
/// lo cual rompe ciertos patrones de EF Core. El archivo temporal garantiza
/// el mismo comportamiento que producción.
/// </summary>
public sealed class DatabaseFixture : IDisposable
{
    public InventarioDbContext Context { get; }
    private readonly string _dbPath;

    public DatabaseFixture()
    {
        _dbPath = Path.Combine(
            Path.GetTempPath(),
            $"teejosh_test_{Guid.NewGuid():N}.db");

        var options = new DbContextOptionsBuilder<InventarioDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;

        Context = new InventarioDbContext(options);
        Context.Database.Migrate();
    }

    /// <summary>
    /// Limpia todas las tablas entre tests para evitar contaminación de datos.
    /// El orden respeta las foreign keys: detalles y ventas antes que product.
    /// </summary>
    public void LimpiarDatos()
    {
        // Ventas (FK → product via sale_detail)
        Context.Database.ExecuteSqlRaw("DELETE FROM sale_detail");
        Context.Database.ExecuteSqlRaw("DELETE FROM sale");

        // Detalles (FK → product, ON DELETE CASCADE pero limpiamos explícitamente)
        Context.Database.ExecuteSqlRaw("DELETE FROM hot_wheels");
        Context.Database.ExecuteSqlRaw("DELETE FROM funko");
        Context.Database.ExecuteSqlRaw("DELETE FROM tcg");
        Context.Database.ExecuteSqlRaw("DELETE FROM toy");
        Context.Database.ExecuteSqlRaw("DELETE FROM varios");

        // Tabla principal
        Context.Database.ExecuteSqlRaw("DELETE FROM product");

        // Reset autoincrement para tests deterministas
        Context.Database.ExecuteSqlRaw(
            "DELETE FROM sqlite_sequence WHERE name IN ('product','sale','sale_detail')");

        // Limpiar change tracker para evitar entidades en estado stale
        Context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        Context.ChangeTracker.Clear();
        Context.Dispose();

        // SQLite en Windows retiene el file handle hasta que el pool
        // libera las conexiones. Sin esto, File.Delete lanza IOException.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
        {
            try { File.Delete(_dbPath); }
            catch { /* ignorar — el OS limpia los temporales al reiniciar */ }
        }
    }
}