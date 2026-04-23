using AUX9.Entities;

namespace AUX9.Services
{
    public interface ITerceroService
    {
        Task<BusinessLogicResponse> ObtenerTercerosAsync();

        Task<BusinessLogicResponse> ObtenerMovimientosAsync(
            int terceroId,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? periodoId,
            string? estadoCodigo,
            int pagina,
            string? usuario
        );
    }
}
