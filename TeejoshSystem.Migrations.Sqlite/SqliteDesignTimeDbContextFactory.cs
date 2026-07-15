using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Migrations.Sqlite;

public sealed class SqliteDesignTimeDbContextFactory : IDesignTimeDbContextFactory<InventarioDbContext>
{
    public InventarioDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InventarioDbContext>()
            .UseSqlite("Data Source=teejosh-design-time.db", provider =>
                provider.MigrationsAssembly(typeof(SqliteDesignTimeDbContextFactory).Assembly.FullName))
            .Options;
        return new InventarioDbContext(options);
    }
}