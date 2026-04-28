using AUX10.Entities;
using AUX10.Repository;

namespace AUX10.Services
{
	public class ReporteService : IReporteService
	{
		private readonly IReporteRepository _repository;
		private readonly IBitacoraService _bitacora;
		private readonly ILogger<ReporteService> _logger;

		private const int ElementosPorPagina = 10;

		public ReporteService(
			IReporteRepository repository,
			IBitacoraService bitacora,
			ILogger<ReporteService> logger)
		{
			_repository = repository;
			_bitacora = bitacora;
			_logger = logger;
		}

		public async Task<BusinessLogicResponse> ObtenerMovimientosAsync(
			string? centroCosto,
			DateTime? fechaInicio,
			DateTime? fechaFin,
			string? periodoId,
			string? estado,
			int pagina,
			string? usuario)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(centroCosto))
				{
					return new BusinessLogicResponse
					{
						StatusCode = 400,
						Message = "Debe seleccionar un centro de costo."
					};
				}

				if (pagina < 1) pagina = 1;

				var (movimientos, totalRegistros) =
					await _repository.ObtenerMovimientosAsync(
						centroCosto,
						fechaInicio,
						fechaFin,
						periodoId,
						estado,
						pagina,
						ElementosPorPagina
					);

				var lista = movimientos.ToList();
				var totalMonto = lista.Sum(x => x.Monto);

				var totalPaginas = totalRegistros == 0
					? 1
					: (int)Math.Ceiling(totalRegistros / (double)ElementosPorPagina);

				
				_ = _bitacora.RegistrarAsync(
					$"El usuario consulta movimientos por centro de costo: {centroCosto}",
					usuario
				);

				return new BusinessLogicResponse
				{
					StatusCode = 200,
					Message = "OK",
					ResponseObject = new ReporteCentroCostoResponse
					{
						Movimientos = lista,
						Total = totalMonto,
						TotalRegistros = totalRegistros,
						Pagina = pagina,
						TotalPaginas = totalPaginas
					}
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en reporte centro costo");

				return new BusinessLogicResponse
				{
					StatusCode = 500,
					Message = "Error interno al generar el reporte."
				};
			}
		}
		public async Task<BusinessLogicResponse> ListarCentrosCostoAsync(string usuario)
		{
			var data = await _repository.ListarCentrosCostoAsync();
			_ = _bitacora.RegistrarAsync("El usuario consulta listado de centros de costo", usuario);
			return new BusinessLogicResponse
			{
				StatusCode = 200,
				ResponseObject = data
			};
		}
	}
}

