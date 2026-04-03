using Microsoft.EntityFrameworkCore;
using TeejoshInventario.Domain.Entities;
using TeejoshInventario.Domain.Entities.Catalogos;
using TeejoshInventario.Domain.Entities.Detalles;

namespace TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence
{
    public class InventarioDbContext : DbContext
    {
        public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options)
        {
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventarioDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
