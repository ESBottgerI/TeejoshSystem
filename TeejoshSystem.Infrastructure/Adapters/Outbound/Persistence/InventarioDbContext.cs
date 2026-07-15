using Microsoft.EntityFrameworkCore;

using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence
{
    public class InventarioDbContext : DbContext
    {
        public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options) { }

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

            // Cada detalle se almacena en su propia tabla. Este ajuste final evita que las
            // convenciones de herencia reintroduzcan un discriminador innecesario.
            modelBuilder.Entity<HotWheelsDetalle>().HasNoDiscriminator();
            modelBuilder.Entity<FunkoDetalle>().HasNoDiscriminator();
            modelBuilder.Entity<TcgDetalle>().HasNoDiscriminator();
            modelBuilder.Entity<ToyDetalle>().HasNoDiscriminator();
            modelBuilder.Entity<VariosDetalle>().HasNoDiscriminator();

            base.OnModelCreating(modelBuilder);
        }
    }
}
