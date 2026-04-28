using AUX8Correcta.Entities;

namespace AUX8Correcta.Repository
{
    public interface Itercerosrepository
    {
        Task<IEnumerable<PeriodoContableDto>> ObtenerPeriodosAsync();
        Task<IEnumerable<LineaAsientoTerceroDto>> ObtenerLineasAsientoPorPeriodoAsync(int periodoId);
        Task<DetalleAsientoTerceroDto?> ObtenerDetalleAsientoAsync(int detalleId);
        Task<IEnumerable<TerceroDto>> ObtenerTercerosActivosAsync();
        Task<IEnumerable<DistribucionTerceroDto>> ObtenerDistribucionesAsync(int detalleId);
        Task<DistribucionTerceroDto?> ObtenerDistribucionPorIdAsync(int id, int detalleId);
        Task<bool> ExisteDistribucionAsync(int detalleId, int terceroId);
        Task AgregarDistribucionAsync(int detalleId, int terceroId, decimal monto, string? usuario);
        Task EliminarDistribucionAsync(int id, int detalleId);
    }
}
