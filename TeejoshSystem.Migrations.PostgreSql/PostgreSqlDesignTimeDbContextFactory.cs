using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Migrations.PostgreSql;

public sealed class PostgreSqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<InventarioDbContext>
{
    public InventarioDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InventarioDbContext>()
            .UseNpgsql("Host=localhost;Database=teejosh_design;Username=postgres;Password=postgres", provider =>
                provider.MigrationsAssembly(typeof(PostgreSqlDesignTimeDbContextFactory).Assembly.FullName))
            .Options;
        return new InventarioDbContext(options);
    }
}