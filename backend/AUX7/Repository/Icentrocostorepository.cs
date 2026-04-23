using AUX7.Entities;

namespace AUX7.Repository
{
    public interface ICentroCostoRepository
    {
        Task<IEnumerable<PeriodoContableDto>> ObtenerPeriodosAsync();
        Task<IEnumerable<LineaAsientoDto>> ObtenerLineasAsientoPorPeriodoAsync(int periodoId);
        Task<DetalleAsientoDto?> ObtenerDetalleAsientoAsync(int detalleId);
        Task<IEnumerable<CentroCostoDto>> ObtenerCentrosActivosAsync();
        Task<IEnumerable<DistribucionCentroCostoDto>> ObtenerDistribucionesAsync(int detalleId);
        Task<bool> ExisteDistribucionAsync(int detalleId, int centroCostoId);
        Task<bool> CentroCostoExisteYActivoAsync(int centroCostoId);
        Task AgregarDistribucionAsync(int detalleId, int centroCostoId, decimal monto, string? usuario);
        Task EliminarDistribucionAsync(int id, int detalleId);
        Task<DistribucionCentroCostoDto?> ObtenerDistribucionPorIdAsync(int id, int detalleId);
    }
}