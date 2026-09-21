using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly MiCholoDbContext _context;

        public ProveedorRepository(MiCholoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Proveedor>> ObtenerTodasAsync()
        {
            return await _context.Proveedores
                .OrderBy(p => p.RazonSocial)
                .ToListAsync();
        }

        public async Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            return await _context.Proveedores
                .FirstOrDefaultAsync(p => p.IdProveedor == id);
        }

        public async Task CrearAsync(Proveedor proveedor)
        {
            _context.Proveedores.Add(proveedor);

            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Proveedor proveedor)
        {
            _context.Proveedores.Update(proveedor);

            await _context.SaveChangesAsync();
        }
    }
}