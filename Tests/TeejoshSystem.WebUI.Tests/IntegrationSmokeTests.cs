using FluentAssertions;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductos;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;
using TeejoshSystem.Infrastructure.DependencyInjection;
using Xunit;

namespace TeejoshSystem.WebUI.Tests;

public sealed class IntegrationSmokeTests
{
    [Fact]
    public async Task Mediator_ExecutesProductQueryAgainstRealSqlite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Database:Provider"] = "sqlite" }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        services.RemoveAll<DbContextOptions<InventarioDbContext>>();
        services.AddDbContext<InventarioDbContext>(options => options.UseSqlite(connection));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ObtenerProductosQuery).Assembly));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
        await db.Database.EnsureCreatedAsync();
        var result = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new ObtenerProductosQuery());
        result.Should().BeEmpty();
    }
}
