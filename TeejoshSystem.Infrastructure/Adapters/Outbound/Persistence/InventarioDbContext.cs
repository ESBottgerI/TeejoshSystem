using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence
{
    public class InventarioDbContext : DbContext
    {
        private readonly ICurrentUserProvider? _currentUserProvider;

        private static readonly HashSet<Type> EntidadesAuditadas = new()
        {
            typeof(Producto),
            typeof(Venta),
            typeof(Usuario)
        };
        
        // Constructor principal — usado por DI cuando provider está configurado
        public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options) { }

        // Constructor protegido no-genérico — requerido para que clases derivadas
        // (LocalDbContext) puedan pasar sus propias DbContextOptions<T> a la base.
        // EF Core define este patrón exactamente para soportar herencia de DbContext.
        protected InventarioDbContext(DbContextOptions options) : base(options) { }
        // DbSets principales
        public DbSet<Producto> Productos { get; set; }

        public DbSet<HotWheelsDetalle> HotWheelsDetalles { get; set; }
        public DbSet<FunkoDetalle> FunkoDetalles { get; set; }
        public DbSet<TcgDetalle> TcgDetalles { get; set; }
        public DbSet<ToyDetalle> ToyDetalles { get; set; }
        public DbSet<VariosDetalle> VariosDetalles { get; set; }
        public DbSet<HotWheelsCategoria> HotWheelsCategorias { get; set; }
        public DbSet<FunkoSubtipo> FunkoSubtipos { get; set; }
        public DbSet<FunkoCaracteristica> FunkoCaracteristicas { get; set; }
        public DbSet<TcgFranquicia> TcgFranquicias { get; set; }
        public DbSet<TcgExpansion> TcgExpansiones { get; set; }
        public DbSet<TcgPack> TcgPacks { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventarioDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            // Paso 1 — capturar antes de guardar (los IDs aún no existen para Crear)
            var pendientes = CapturarCambiosPendientes();

            // Paso 2 — guardar los cambios reales (SQLite asigna IDs aquí)
            var resultado = await base.SaveChangesAsync(cancellationToken);

            // Paso 3 — ahora los IDs ya están asignados, construir entradas de auditoría
            if (pendientes.Count > 0)
            {
                var usuario = _currentUserProvider?.UsuarioActual;

                foreach (var pendiente in pendientes)
                {
                    // Leer el ID real post-save
                    var idReal = pendiente.Entry.Properties
                        .FirstOrDefault(p => p.Metadata.Name == "Id")?
                        .CurrentValue?.ToString() ?? "desconocido";

                    AuditLogs.Add(new AuditLog(
                        pendiente.NombreEntidad,
                        idReal,
                        pendiente.Accion.ToString(),
                        usuario,
                        pendiente.CambiosJson));
                }

                // Paso 4 — guardar las entradas de auditoría
                await base.SaveChangesAsync(cancellationToken);
            }

            return resultado;
        }

        private List<AuditPendiente> CapturarCambiosPendientes()
        {
            var resultado = new List<AuditPendiente>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State is not (EntityState.Added
                    or EntityState.Modified
                    or EntityState.Deleted))
                    continue;

                if (!EntidadesAuditadas.Contains(entry.Entity.GetType()))
                    continue;

                var accion = entry.State switch
                {
                    EntityState.Added => AccionAuditoria.Crear,
                    EntityState.Modified => AccionAuditoria.Actualizar,
                    EntityState.Deleted => AccionAuditoria.Eliminar,
                    _ => AccionAuditoria.Actualizar
                };

                var cambios = new Dictionary<string, object?>();

                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.Name == "Discriminator") continue;

                    switch (accion)
                    {
                        case AccionAuditoria.Crear:
                            cambios[prop.Metadata.Name] = new
                            {
                                anterior = (object?)null,
                                nuevo = prop.CurrentValue
                            };
                            break;

                        case AccionAuditoria.Eliminar:
                            cambios[prop.Metadata.Name] = new
                            {
                                anterior = prop.OriginalValue,
                                nuevo = (object?)null
                            };
                            break;

                        case AccionAuditoria.Actualizar:
                            if (prop.IsModified)
                                cambios[prop.Metadata.Name] = new
                                {
                                    anterior = prop.OriginalValue,
                                    nuevo = prop.CurrentValue
                                };
                            break;
                    }
                }

                resultado.Add(new AuditPendiente(
                    entry,
                    entry.Entity.GetType().Name,
                    accion,
                    cambios.Count > 0 ? JsonSerializer.Serialize(cambios) : null));
            }

            return resultado;
        }

        private sealed record AuditPendiente(
            EntityEntry Entry,
            string NombreEntidad,
            AccionAuditoria Accion,
            string? CambiosJson);
    }
}