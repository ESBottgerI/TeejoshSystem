using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.ValueObjects;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Auditoria;

public class AuditLogIntegrationTests
{
    private static InventarioDbContext CrearContexto(string? usuario = "admin")
    {
        var currentUser = Substitute.For<ICurrentUserProvider>();
        currentUser.UsuarioActual.Returns(usuario);

        var options = new DbContextOptionsBuilder<InventarioDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new InventarioDbContext(options, currentUser);
    }

    [Fact]
    public async Task CrearProducto_GeneraEntradaAuditLog_ConAccionCrear()
    {
        await using var context = CrearContexto("admin");

        var producto = new Producto(
            TipoProducto.HotWheels,
            new NombreProducto("Camaro 1969"),
            new Precio(25m),
            new Unidades(3));

        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        var entradas = await context.AuditLogs.ToListAsync();

        entradas.Should().NotBeEmpty();

        var entradaProducto = entradas
            .FirstOrDefault(e => e.Entidad == "Producto" && e.Accion == "Crear");

        entradaProducto.Should().NotBeNull();
        entradaProducto!.Usuario.Should().Be("admin");
        entradaProducto.Accion.Should().Be("Crear");
        entradaProducto.Entidad.Should().Be("Producto");
    }

    [Fact]
    public async Task EliminarProducto_GeneraEntradaAuditLog_ConAccionEliminar()
    {
        await using var context = CrearContexto("admin");

        var producto = new Producto(
            TipoProducto.HotWheels,
            new NombreProducto("Camaro 1969"),
            new Precio(25m),
            new Unidades(3));

        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        // Limpiar entradas del Crear para aislar el test
        var entradasCrear = await context.AuditLogs.ToListAsync();
        context.AuditLogs.RemoveRange(entradasCrear);
        await context.SaveChangesAsync();

        // Ahora eliminar
        context.Productos.Remove(producto);
        await context.SaveChangesAsync();

        var entradas = await context.AuditLogs.ToListAsync();

        var entradaEliminar = entradas
            .FirstOrDefault(e => e.Entidad == "Producto" && e.Accion == "Eliminar");

        entradaEliminar.Should().NotBeNull();
        entradaEliminar!.Usuario.Should().Be("admin");
        entradaEliminar.Accion.Should().Be("Eliminar");
    }

    [Fact]
    public async Task ActualizarProducto_GeneraEntradaAuditLog_ConAccionActualizar()
    {
        await using var context = CrearContexto("admin");

        var producto = new Producto(
            TipoProducto.HotWheels,
            new NombreProducto("Camaro 1969"),
            new Precio(25m),
            new Unidades(3));

        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        // Limpiar entradas anteriores
        var entradasAntes = await context.AuditLogs.ToListAsync();
        context.AuditLogs.RemoveRange(entradasAntes);
        await context.SaveChangesAsync();

        // Actualizar precio
        producto.ActualizarDatos(
            new NombreProducto("Camaro 1969"),
            new Precio(30m),
            new Unidades(3));

        context.Productos.Update(producto);
        await context.SaveChangesAsync();

        var entradas = await context.AuditLogs.ToListAsync();

        var entradaActualizar = entradas
            .FirstOrDefault(e => e.Entidad == "Producto" && e.Accion == "Actualizar");

        entradaActualizar.Should().NotBeNull();
        entradaActualizar!.Accion.Should().Be("Actualizar");
        entradaActualizar.Cambios.Should().NotBeNull();
    }

    [Fact]
    public async Task AuditLog_CapturaNombreUsuario_DelCurrentUserProvider()
    {
        await using var context = CrearContexto("vendedor1");

        var producto = new Producto(
            TipoProducto.Funko,
            new NombreProducto("Funko Goku"),
            new Precio(45m),
            new Unidades(1));

        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        var entrada = await context.AuditLogs
            .FirstOrDefaultAsync(e => e.Entidad == "Producto");

        entrada.Should().NotBeNull();
        entrada!.Usuario.Should().Be("vendedor1");
    }
}