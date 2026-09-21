using MiCholoMVC.Models;
using MiCholoMVC.Repositories;

namespace MiCholoMVC.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repository;

        public CategoriaService(ICategoriaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Categoria>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Categoria categoria)
        {
            categoria.Estado = true;

            await _repository.CrearAsync(categoria);
        }

        public async Task ActualizarAsync(Categoria categoria)
        {
            await _repository.ActualizarAsync(categoria);
        }
    }
}