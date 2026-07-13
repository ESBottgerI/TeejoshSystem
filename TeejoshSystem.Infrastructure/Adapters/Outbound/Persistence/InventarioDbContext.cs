using Microsoft.EntityFrameworkCore;

using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence
{
    public class InventarioDbContext : DbContext
    {
        // Constructor principal — usado por DI cuando provider está configurado
        public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options) { }

        // Constructor protegido no-genérico — requerido para que clases derivadas
        // (LocalDbContext) puedan pasar sus propias DbContextOptions<T> a la base.
        // EF Core define este patrón exactamente para soportar herencia de DbContext.
        protected InventarioDbContext(DbContextOptions options) : base(options) { }

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventarioDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
