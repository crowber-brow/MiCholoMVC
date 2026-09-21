using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public interface ICajaRepository
    {
        Task<List<Caja>> ObtenerTodasAsync();
        Task<Caja?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Caja caja);
        Task ActualizarAsync(Caja caja);

        Task<List<MovimientoCaja>> ObtenerMovimientosAsync(int idCaja);
        Task CrearMovimientoAsync(MovimientoCaja movimiento);
    }
}