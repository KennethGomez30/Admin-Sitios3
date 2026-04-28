using AUX10.Entities;

namespace AUX10.Repository
{
	public interface IReporteRepository
	{
		Task<(IEnumerable<MovimientoCentroCostoDto> Movimientos, int Total)>
		ObtenerMovimientosAsync(
			string? centroCosto,
			DateTime? fechaInicio,
			DateTime? fechaFin,
			string? periodoId,
			string? estado,
			int pagina,
			int elementosPorPagina
		);

		Task<IEnumerable<CentroCostoDto>> ListarCentrosCostoAsync();
	}
}
