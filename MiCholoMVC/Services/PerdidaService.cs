using MiCholoMVC.Models;
using MiCholoMVC.Repositories;

namespace MiCholoMVC.Services
{
    public class PerdidaService : IPerdidaService
    {
        private readonly IPerdidaRepository _repository;

        public PerdidaService(
            IPerdidaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Perdida>> ObtenerTodosAsync()
        {
            return await _repository.ObtenerTodosAsync();
        }

        public async Task<Perdida?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Perdida perdida)
        {
            perdida.Fecha = DateTime.Now;

            await _repository.CrearAsync(perdida);
        }
    }
}