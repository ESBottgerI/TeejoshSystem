using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Infrastructure.Tests.Fixtures;

/// <summary>
/// Fixture compartida por todos los tests de Infrastructure.
/// Crea una BD SQLite real en archivo temporal por cada clase de test,
/// aplica las migraciones reales, y la elimina al finalizar.
///
/// Por qué archivo temporal y no :memory::
/// - SQLite :memory: no soporta múltiples conexiones (necesario para
///   crear el contexto y verificar en el mismo test)
/// - El archivo temporal garantiza el mismo comportamiento que producción
/// </summary>
public sealed class DatabaseFixture : IDisposable
{
    public InventarioDbContext Context { get; }
    private readonly string _dbPath;

    public DatabaseFixture()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"teejosh_test_{Guid.NewGuid():N}.db");

        var options = new DbContextOptionsBuilder<InventarioDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;

        Context = new InventarioDbContext(options);

        // Aplica las migraciones reales — mismo flujo que producción
        Context.Database.Migrate();
    }

    /// <summary>
    /// Limpia todas las tablas relevantes entre tests para evitar
    /// contaminación de datos sin recrear la BD completa.
    /// El orden importa por las foreign keys.
    /// </summary>
    public void LimpiarDatos()
    {
        // Detalles primero (FK → product)
        Context.Database.ExecuteSqlRaw("DELETE FROM hot_wheels");
        Context.Database.ExecuteSqlRaw("DELETE FROM funko");
        Context.Database.ExecuteSqlRaw("DELETE FROM tcg");
        Context.Database.ExecuteSqlRaw("DELETE FROM toy");
        Context.Database.ExecuteSqlRaw("DELETE FROM varios");

        // Ventas (si existen)
        Context.Database.ExecuteSqlRaw("DELETE FROM sale_detail");
        Context.Database.ExecuteSqlRaw("DELETE FROM sale");

        // Tabla principal
        Context.Database.ExecuteSqlRaw("DELETE FROM product");

        // Reset autoincrement para tests deterministas
        Context.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name IN ('product','sale')");
    }

    public void Dispose()
    {
        Context.Dispose();
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
