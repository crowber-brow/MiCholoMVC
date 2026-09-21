using MiCholoMVC.Models;
using MiCholoMVC.Repositories;

namespace MiCholoMVC.Services
{
    public class MovimientoInventarioService
        : IMovimientoInventarioService
    {
        private readonly IMovimientoInventarioRepository
            _repository;

        public MovimientoInventarioService(
            IMovimientoInventarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MovimientoInventario>>
            ObtenerTodosAsync()
        {
            return await _repository.ObtenerTodosAsync();
        }

        public async Task<MovimientoInventario?>
            ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(
            MovimientoInventario movimiento)
        {
            movimiento.Fecha = DateTime.Now;

            await _repository.CrearAsync(movimiento);
        }
    }
}