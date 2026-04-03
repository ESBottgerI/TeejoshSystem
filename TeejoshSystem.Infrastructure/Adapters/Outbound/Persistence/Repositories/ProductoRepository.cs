using Microsoft.EntityFrameworkCore;
using TeejoshInventario.Domain.Entities;
using TeejoshInventario.Domain.Entities.Detalles;
using TeejoshInventario.Domain.Enums;
using TeejoshInventario.Domain.Ports.Outbound.Repositories;

namespace TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Repositories
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

        public async Task<IReadOnlyList<Producto>> SearchByNameAsync(string nombre)
        {
            return await _context.Productos
                .AsNoTracking()
                .Where(p => p.Nombre.Value.Contains(nombre))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Producto>> SearchAsync(string? nombre, TipoProducto? tipo)
        {
            var query = _context.Productos.AsNoTracking();

            // Filtrar por nombre
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(p => p.Nombre.Value.Contains(nombre));
            }

            // Si NO hay filtro de tipo, devolver todos
            if (!tipo.HasValue)
            {
                return await query.ToListAsync();
            }

            // Si HAY filtro de tipo, hacer join con tabla correspondiente
            var productos = await query.ToListAsync();

            // Filtrar en memoria según el tipo
            var productosFiltrados = new List<Producto>();

            foreach (var producto in productos)
            {
                var tipoProducto = await GetTipoProductoAsync(producto.Id);

                if (tipoProducto == tipo.Value)
                {
                    productosFiltrados.Add(producto);
                }
            }

            return productosFiltrados;
        }

        public async Task<(Producto producto, object? detalle)> GetByIdWithDetalleAsync(
            int id,
            TipoProducto tipo)
        {
            var producto = await GetByIdAsync(id);

            if (producto is null)
                return (null, null);

            object? detalle = tipo switch
            {
                TipoProducto.HotWheels => await _context.HotWheelsDetalles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.ProductoId == id),

                TipoProducto.Funko => await _context.FunkoDetalles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.ProductoId == id),

                TipoProducto.Tcg => await _context.TcgDetalles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.ProductoId == id),

                TipoProducto.Toy => await _context.ToyDetalles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.ProductoId == id),

                TipoProducto.Varios => await _context.VariosDetalles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.ProductoId == id),

                _ => null
            };

            return (producto, detalle);
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

        public async Task<TipoProducto?> GetTipoProductoAsync(int id)
        {
            if (await _context.HotWheelsDetalles.AnyAsync(d => d.ProductoId == id))
                return TipoProducto.HotWheels;

            if (await _context.FunkoDetalles.AnyAsync(d => d.ProductoId == id))
                return TipoProducto.Funko;

            if (await _context.TcgDetalles.AnyAsync(d => d.ProductoId == id))
                return TipoProducto.Tcg;

            if (await _context.ToyDetalles.AnyAsync(d => d.ProductoId == id))
                return TipoProducto.Toy;

            if (await _context.VariosDetalles.AnyAsync(d => d.ProductoId == id))
                return TipoProducto.Varios;

            return null;
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