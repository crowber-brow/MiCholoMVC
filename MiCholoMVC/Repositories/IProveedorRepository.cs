using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public interface IProveedorRepository
    {
        Task<List<Proveedor>> ObtenerTodasAsync();
        Task<Proveedor?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Proveedor proveedor);
        Task ActualizarAsync(Proveedor proveedor);
    }
}