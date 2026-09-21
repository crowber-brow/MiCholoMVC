using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public class CajaRepository : ICajaRepository
    {
        private readonly MiCholoDbContext _context;

        public CajaRepository(MiCholoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Caja>> ObtenerTodasAsync()
        {
            return await _context.Cajas
                .OrderByDescending(c => c.IdCaja)
                .ToListAsync();
        }

        public async Task<Caja?> ObtenerPorIdAsync(int id)
        {
            return await _context.Cajas
                .FirstOrDefaultAsync(c => c.IdCaja == id);
        }

        public async Task CrearAsync(Caja caja)
        {
            _context.Cajas.Add(caja);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Caja caja)
        {
            _context.Cajas.Update(caja);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MovimientoCaja>> ObtenerMovimientosAsync(int idCaja)
        {
            return await _context.MovimientosCaja
                .Where(m => m.IdCaja == idCaja)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task CrearMovimientoAsync(MovimientoCaja movimiento)
        {
            _context.MovimientosCaja.Add(movimiento);
            await _context.SaveChangesAsync();
        }
    }
}