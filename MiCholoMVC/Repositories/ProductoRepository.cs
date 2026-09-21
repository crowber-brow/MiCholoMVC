using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly MiCholoDbContext _context;

        public ProductoRepository(MiCholoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Producto>> ObtenerTodasAsync()
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.IdProducto == id);
        }

        public async Task CrearAsync(Producto producto)
        {
            _context.Productos.Add(producto);

            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Producto producto)
        {
            _context.Productos.Update(producto);

            await _context.SaveChangesAsync();
        }
    }
}