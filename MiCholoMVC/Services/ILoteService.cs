using MiCholoMVC.Models;

namespace MiCholoMVC.Services
{
    public interface ILoteService
    {
        Task<List<Lote>> ObtenerTodasAsync();
        Task<Lote?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Lote lote);
        Task ActualizarAsync(Lote lote);
    }
}