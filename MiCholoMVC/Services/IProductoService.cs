using MiCholoMVC.Models;

namespace MiCholoMVC.Services
{
    public interface IProductoService
    {
        Task<List<Producto>> ObtenerTodasAsync();
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Producto producto);
        Task ActualizarAsync(Producto producto);
    }
}