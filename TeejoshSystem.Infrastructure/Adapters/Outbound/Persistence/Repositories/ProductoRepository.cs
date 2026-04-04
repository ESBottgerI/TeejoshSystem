using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly InventarioDbContext _context;

        public ProductoRepository(InventarioDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Producto>> GetAllAsync()
        {
            return await _context.Productos
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Producto?> GetByIdAsync(int id)
        {
            return await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IReadOnlyList<Producto>> SearchAsync(string? nombre, TipoProducto? tipo)
        {
            var query = _context.Productos.AsNoTracking();

            // Filtrar por nombre
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(p => p.Nombre.Value.Contains(nombre));
            }

            // Si hay filtro de tipo
            if (tipo.HasValue)
            {
                query = query.Where(p => p.Tipo == tipo.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Producto?> GetByIdWithDetalleAsync(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto is null) return null;

            ProductoDetalle? detalle = producto.Tipo switch
            {
                TipoProducto.HotWheels => await _context.HotWheelsDetalles
                    .FirstOrDefaultAsync(d => d.ProductoId == id),
                TipoProducto.Funko => await _context.FunkoDetalles
                    .FirstOrDefaultAsync(d => d.ProductoId == id),
                TipoProducto.Tcg => await _context.TcgDetalles
                    .FirstOrDefaultAsync(d => d.ProductoId == id),
                TipoProducto.Toy => await _context.ToyDetalles
                    .FirstOrDefaultAsync(d => d.ProductoId == id),
                TipoProducto.Varios => await _context.VariosDetalles
                    .FirstOrDefaultAsync(d => d.ProductoId == id),
                _ => null
            };

            if (detalle is not null)
                producto.AsignarDescripcion(detalle);

            return producto;
        }

        public async Task<int> AddAsync(Producto producto)
        {
            await _context.Productos.AddAsync(producto);
            await _context.SaveChangesAsync();
            return producto.Id;
        }

        public async Task UpdateAsync(Producto producto)
        {
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Producto producto)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<int> productoIds)
        {
            var productos = await _context.Productos
                .Where(p => productoIds.Contains(p.Id))
                .ToListAsync();

            _context.Productos.RemoveRange(productos);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Productos
                .AnyAsync(p => p.Id == id);
        }

        // Metodos auxiliares para manejar detalles
        public async Task AddHotWheelsDetalleAsync(HotWheelsDetalle detalle)
        {
            await _context.HotWheelsDetalles.AddAsync(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task AddFunkoDetalleAsync(FunkoDetalle detalle)
        {
            await _context.FunkoDetalles.AddAsync(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task AddTcgDetalleAsync(TcgDetalle detalle)
        {
            await _context.TcgDetalles.AddAsync(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task AddToyDetalleAsync(ToyDetalle detalle)
        {
            await _context.ToyDetalles.AddAsync(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task AddVariosDetalleAsync(VariosDetalle detalle)
        {
            await _context.VariosDetalles.AddAsync(detalle);
            await _context.SaveChangesAsync();
        }

        // Métodos para ACTUALIZAR detalles
        public async Task UpdateHotWheelsDetalleAsync(HotWheelsDetalle detalle)
        {
            _context.HotWheelsDetalles.Update(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFunkoDetalleAsync(FunkoDetalle detalle)
        {
            _context.FunkoDetalles.Update(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTcgDetalleAsync(TcgDetalle detalle)
        {
            _context.TcgDetalles.Update(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateToyDetalleAsync(ToyDetalle detalle)
        {
            _context.ToyDetalles.Update(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVariosDetalleAsync(VariosDetalle detalle)
        {
            _context.VariosDetalles.Update(detalle);
            await _context.SaveChangesAsync();
        }
    }
}