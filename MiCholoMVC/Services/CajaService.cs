using MiCholoMVC.Models;
using MiCholoMVC.Repositories;

namespace MiCholoMVC.Services
{
    public class CajaService : ICajaService
    {
        private readonly ICajaRepository _repository;

        public CajaService(ICajaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Caja>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        public async Task<Caja?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Caja caja)
        {
            await _repository.CrearAsync(caja);
        }

        public async Task ActualizarAsync(Caja caja)
        {
            await _repository.ActualizarAsync(caja);
        }

        public async Task<List<MovimientoCaja>> ObtenerMovimientosAsync(int idCaja)
        {
            return await _repository.ObtenerMovimientosAsync(idCaja);
        }

        public async Task CrearMovimientoAsync(MovimientoCaja movimiento)
        {
            await _repository.CrearMovimientoAsync(movimiento);
        }
    }
}