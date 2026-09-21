using MiCholoMVC.Models;
using MiCholoMVC.Repositories;

namespace MiCholoMVC.Services
{
    public class LoteService : ILoteService
    {
        private readonly ILoteRepository _repository;

        public LoteService(ILoteRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Lote>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        public async Task<Lote?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Lote lote)
        {
            lote.Estado = true;

            await _repository.CrearAsync(lote);
        }

        public async Task ActualizarAsync(Lote lote)
        {
            await _repository.ActualizarAsync(lote);
        }
    }
}