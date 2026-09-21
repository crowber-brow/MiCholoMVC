using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public interface ILoteRepository
    {
        Task<List<Lote>> ObtenerTodasAsync();
        Task<Lote?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Lote lote);
        Task ActualizarAsync(Lote lote);
    }
}