using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence
{
    /// <summary>
    /// Factory de design-time para LocalDbContext.
    /// Usada exclusivamente por el CLI de EF Core al generar migraciones:
    ///   dotnet ef migrations add ... --context LocalDbContext
    ///
    /// Siempre apunta a SQLite local — LocalDbContext nunca usa PostgreSQL.
    /// </summary>
    public class LocalDbContextFactory : IDesignTimeDbContextFactory<LocalDbContext>
    {
        public LocalDbContext CreateDbContext(string[] args)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TeejoshSystem",
                "inventario.db");

            var options = new DbContextOptionsBuilder<LocalDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            return new LocalDbContext(options);
        }
    }
}