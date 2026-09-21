using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public interface IMovimientoInventarioRepository
    {
        Task<List<MovimientoInventario>> ObtenerTodosAsync();

        Task<MovimientoInventario?> ObtenerPorIdAsync(int id);

        Task CrearAsync(MovimientoInventario movimiento);
    }
}