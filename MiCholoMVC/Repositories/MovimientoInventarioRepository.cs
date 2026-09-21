using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public class MovimientoInventarioRepository
        : IMovimientoInventarioRepository
    {
        private readonly MiCholoDbContext _context;

        public MovimientoInventarioRepository(
            MiCholoDbContext context)
        {
            _context = context;
        }

        public async Task<List<MovimientoInventario>>
            ObtenerTodosAsync()
        {
            return await _context.MovimientosInventario
                .Include(m => m.Producto)
                .Include(m => m.Lote)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<MovimientoInventario?>
            ObtenerPorIdAsync(int id)
        {
            return await _context.MovimientosInventario
                .Include(m => m.Producto)
                .Include(m => m.Lote)
                .FirstOrDefaultAsync(
                    m => m.IdMovimiento == id);
        }

        public async Task CrearAsync(
            MovimientoInventario movimiento)
        {
            _context.MovimientosInventario.Add(movimiento);

            await _context.SaveChangesAsync();
        }
    }
}