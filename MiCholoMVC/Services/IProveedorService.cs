using MiCholoMVC.Models;

namespace MiCholoMVC.Services
{
    public interface IProveedorService
    {
        Task<List<Proveedor>> ObtenerTodasAsync();
        Task<Proveedor?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Proveedor proveedor);
        Task ActualizarAsync(Proveedor proveedor);
    }
}