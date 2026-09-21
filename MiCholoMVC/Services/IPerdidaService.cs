using MiCholoMVC.Models;

namespace MiCholoMVC.Services
{
    public interface IPerdidaService
    {
        Task<List<Perdida>> ObtenerTodosAsync();

        Task<Perdida?> ObtenerPorIdAsync(int id);

        Task CrearAsync(Perdida perdida);
    }
}