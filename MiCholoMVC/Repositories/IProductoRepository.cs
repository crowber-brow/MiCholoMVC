using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public interface IProductoRepository
    {
        Task<List<Producto>> ObtenerTodasAsync();
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Producto producto);
        Task ActualizarAsync(Producto producto);
    }
}