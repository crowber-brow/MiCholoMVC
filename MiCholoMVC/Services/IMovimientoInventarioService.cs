using MiCholoMVC.Models;

namespace MiCholoMVC.Services
{
    public interface IMovimientoInventarioService
    {
        Task<List<MovimientoInventario>> ObtenerTodosAsync();

        Task<MovimientoInventario?> ObtenerPorIdAsync(int id);

        Task CrearAsync(MovimientoInventario movimiento);
    }
}