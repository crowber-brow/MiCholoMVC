using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public interface IPerdidaRepository
    {
        Task<List<Perdida>> ObtenerTodosAsync();

        Task<Perdida?> ObtenerPorIdAsync(int id);

        Task CrearAsync(Perdida perdida);
    }
}