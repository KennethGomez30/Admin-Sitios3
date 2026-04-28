using AUX10.Entities;

namespace AUX10.Services
{
	public interface IReporteService
	{
		Task<BusinessLogicResponse> ObtenerMovimientosAsync(
			string? centroCosto,
			DateTime? fechaInicio,
			DateTime? fechaFin,
			string? periodoId,
			string? estado,
			int pagina,
			string? usuario
		);

		Task<BusinessLogicResponse> ListarCentrosCostoAsync(string usuario);
	}
}
