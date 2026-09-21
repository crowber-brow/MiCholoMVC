using MiCholoMVC.Models;
using MiCholoMVC.Repositories;

namespace MiCholoMVC.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IProveedorRepository _repository;

        public ProveedorService(IProveedorRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Proveedor>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        public async Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Proveedor proveedor)
        {
            proveedor.Estado = true;

            await _repository.CrearAsync(proveedor);
        }

        public async Task ActualizarAsync(Proveedor proveedor)
        {
            await _repository.ActualizarAsync(proveedor);
        }
    }
}