using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public class LoteRepository : ILoteRepository
    {
        private readonly MiCholoDbContext _context;

        public LoteRepository(MiCholoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Lote>> ObtenerTodasAsync()
        {
            return await _context.Lotes
                .Include(l => l.Producto)
                .OrderBy(l => l.FechaVencimiento)
                .ToListAsync();
        }

        public async Task<Lote?> ObtenerPorIdAsync(int id)
        {
            return await _context.Lotes
                .Include(l => l.Producto)
                .FirstOrDefaultAsync(l => l.IdLote == id);
        }

        public async Task CrearAsync(Lote lote)
        {
            _context.Lotes.Add(lote);

            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Lote lote)
        {
            _context.Lotes.Update(lote);

            await _context.SaveChangesAsync();
        }
    }
}