using MiCholoMVC.Models;

namespace MiCholoMVC.Services
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> ObtenerTodasAsync();
        Task<Categoria?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Categoria categoria);
        Task ActualizarAsync(Categoria categoria);
    }
}