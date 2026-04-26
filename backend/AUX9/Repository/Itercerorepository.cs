using AUX9.Entities;

namespace AUX9.Repository
{
    public interface ITerceroRepository
    {
        Task<IEnumerable<TerceroDto>> ObtenerTercerosAsync();

        Task<(IEnumerable<MovimientoTerceroDto> Movimientos, int Total)> ObtenerMovimientosPorTerceroAsync(
            int terceroId,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? periodoId,
            string? estadoCodigo,
            int pagina,
            int elementosPorPagina
        );
    }
}