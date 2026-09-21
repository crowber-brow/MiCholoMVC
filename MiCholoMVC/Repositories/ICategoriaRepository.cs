using MiCholoMVC.Models;

namespace MiCholoMVC.Repositories
{
    public interface ICategoriaRepository
    {
        Task<List<Categoria>> ObtenerTodasAsync();
        Task<Categoria?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Categoria categoria);
        Task ActualizarAsync(Categoria categoria);
    }
}