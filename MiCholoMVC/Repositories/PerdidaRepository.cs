using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public class PerdidaRepository : IPerdidaRepository
    {
        private readonly MiCholoDbContext _context;

        public PerdidaRepository(MiCholoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Perdida>> ObtenerTodosAsync()
        {
            return await _context.Perdidas
                .Include(p => p.Lote)
                .ThenInclude(l => l!.Producto)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }

        public async Task<Perdida?> ObtenerPorIdAsync(int id)
        {
            return await _context.Perdidas
                .Include(p => p.Lote)
                .ThenInclude(l => l!.Producto)
                .FirstOrDefaultAsync(
                    p => p.IdPerdida == id);
        }

        public async Task CrearAsync(Perdida perdida)
        {
            _context.Perdidas.Add(perdida);

            await _context.SaveChangesAsync();
        }
    }
}