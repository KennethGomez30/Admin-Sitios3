using System.Data;
using AUX10.Entities;
using Dapper;

namespace AUX10.Repository
{
	public class ReporteRepository : IReporteRepository
	{
		private readonly IDbConnectionFactory _dbConnectionFactory;

		public ReporteRepository(IDbConnectionFactory dbConnectionFactory)
		{
			_dbConnectionFactory = dbConnectionFactory;
		}

		public async Task<(IEnumerable<MovimientoCentroCostoDto> Movimientos, int Total)>
		ObtenerMovimientosAsync(
			string? centroCosto,
			DateTime? fechaInicio,
			DateTime? fechaFin,
			string? periodoId,
			string? estado,
			int pagina,
			int elementosPorPagina)
		{
			using var connection = _dbConnectionFactory.CreateConnection();

			var offset = (pagina - 1) * elementosPorPagina;

			using var multi = await connection.QueryMultipleAsync(
				"sp_reporte_movimientos_centro_costo",
				new
				{
					p_CentroCostoId = centroCosto,
					p_FechaInicio = fechaInicio,
					p_FechaFin = fechaFin,
					p_PeriodoId = periodoId,
					p_EstadoCodigo = estado,
					p_Offset = offset,
					p_Limit = elementosPorPagina
				},
				commandType: CommandType.StoredProcedure
			);

			var movimientos = await multi.ReadAsync<MovimientoCentroCostoDto>();
			var total = await multi.ReadFirstOrDefaultAsync<int>();

			return (movimientos, total);
		}

		public async Task<IEnumerable<CentroCostoDto>> ListarCentrosCostoAsync()
		{
			using var conn = _dbConnectionFactory.CreateConnection();
			return await conn.QueryAsync<CentroCostoDto>(
				"SELECT centrocosto_id AS CentroCostoId, codigo AS Codigo, nombre AS Nombre " +
				"FROM centros_costo WHERE Eliminado = 0 AND estado = 'Activo' ORDER BY nombre"


			);
		}
	}
}

