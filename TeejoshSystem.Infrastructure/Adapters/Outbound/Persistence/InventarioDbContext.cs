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

        // Entidades que sí se auditan. El resto (catálogos, configuración) se ignora
        // para no llenar el audit log de ruido.
        private static readonly HashSet<Type> EntidadesAuditadas = new()
        {
            typeof(Producto),
            typeof(Venta),
            typeof(Usuario)
        };

        public InventarioDbContext(
            DbContextOptions<InventarioDbContext> options,
            ICurrentUserProvider? currentUserProvider = null)
            : base(options)
        {
            _currentUserProvider = currentUserProvider;
        }

        // DbSets principales
        public DbSet<Producto> Productos { get; set; }

        // DbSets de detalles
        public DbSet<HotWheelsDetalle> HotWheelsDetalles { get; set; }
        public DbSet<FunkoDetalle> FunkoDetalles { get; set; }
        public DbSet<TcgDetalle> TcgDetalles { get; set; }
        public DbSet<ToyDetalle> ToyDetalles { get; set; }
        public DbSet<VariosDetalle> VariosDetalles { get; set; }

        // DbSets de catalogos
        public DbSet<HotWheelsCategoria> HotWheelsCategorias { get; set; }
        public DbSet<FunkoSubtipo> FunkoSubtipos { get; set; }
        public DbSet<FunkoCaracteristica> FunkoCaracteristicas { get; set; }
        public DbSet<TcgFranquicia> TcgFranquicias { get; set; }
        public DbSet<TcgExpansion> TcgExpansiones { get; set; }
        public DbSet<TcgPack> TcgPacks { get; set; }

        // DbSets de ventas
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }

        // DbSets de usuarios
        public DbSet<Usuario> Usuarios { get; set; }

        // DbSet de auditoría — NUEVO
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventarioDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            var auditEntries = CapturarCambiosParaAuditoria();

            // Las entradas de auditoría se agregan al mismo ChangeTracker
            // antes de guardar, así se persisten en la misma transacción.
            if (auditEntries.Count > 0)
            {
                foreach (var entry in auditEntries)
                {
                    var log = new AuditLog(
                        entry.Entidad,
                        entry.EntidadId,
                        entry.Accion.ToString(),
                        entry.Usuario,
                        entry.Cambios);

                    AuditLogs.Add(log);
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private List<AuditLogEntryData> CapturarCambiosParaAuditoria()
        {
            var resultado = new List<AuditLogEntryData>();
            var usuario = _currentUserProvider?.UsuarioActual;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Added &&
                    entry.State != EntityState.Modified &&
                    entry.State != EntityState.Deleted)
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

                var entidadId = ObtenerId(entry);
                var cambiosJson = ConstruirJsonCambios(entry, accion);

                resultado.Add(new AuditLogEntryData(
                    entry.Entity.GetType().Name,
                    entidadId,
                    accion,
                    usuario,
                    cambiosJson));
            }

            return resultado;
        }

        private static string ObtenerId(EntityEntry entry)
        {
            var idProperty = entry.Properties
                .FirstOrDefault(p => p.Metadata.Name == "Id");

            if (idProperty is null)
                return "desconocido";

            // Para entidades nuevas el Id aún no está asignado (autoincremental).
            // Se usa el valor actual, que EF resuelve después de guardar.
            return idProperty.CurrentValue?.ToString() ?? "pendiente";
        }

        private static string? ConstruirJsonCambios(EntityEntry entry, AccionAuditoria accion)
        {
            var cambios = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                // Ignorar propiedades de navegación complejas no escalares
                if (property.Metadata.IsShadowProperty() &&
                    property.Metadata.Name == "Discriminator")
                    continue;

                switch (accion)
                {
                    case AccionAuditoria.Crear:
                        cambios[property.Metadata.Name] = new
                        {
                            anterior = (object?)null,
                            nuevo = property.CurrentValue
                        };
                        break;

                    case AccionAuditoria.Eliminar:
                        cambios[property.Metadata.Name] = new
                        {
                            anterior = property.OriginalValue,
                            nuevo = (object?)null
                        };
                        break;

                    case AccionAuditoria.Actualizar:
                        if (property.IsModified)
                        {
                            cambios[property.Metadata.Name] = new
                            {
                                anterior = property.OriginalValue,
                                nuevo = property.CurrentValue
                            };
                        }
                        break;
                }
            }

            if (cambios.Count == 0)
                return null;

            return JsonSerializer.Serialize(cambios);
        }
    }
}