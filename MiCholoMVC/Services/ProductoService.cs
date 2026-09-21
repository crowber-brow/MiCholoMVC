using MiCholoMVC.Models;
using MiCholoMVC.Repositories;

namespace MiCholoMVC.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _repository;

        public ProductoService(IProductoRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Producto>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Producto producto)
        {
            producto.Estado = true;
            producto.FechaRegistro = DateTime.Now;

            await _repository.CrearAsync(producto);
        }

        public async Task ActualizarAsync(Producto producto)
        {
            await _repository.ActualizarAsync(producto);
        }
    }
}