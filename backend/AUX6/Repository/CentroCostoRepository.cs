using System.Data;
using AUX6.Entities;
using Dapper;

namespace AUX6.Repository
{
	public class CentroCostoRepository : ICentroCostoRepository
	{
		private readonly IDbConnectionFactory _db;

		public CentroCostoRepository(IDbConnectionFactory db)
		{
			_db = db;
		}

		public async Task<(IEnumerable<CentroCostoDto>, int)> ListarAsync(int pagina, int pageSize)
		{
			using var conn = _db.CreateConnection();

			var offset = (pagina - 1) * pageSize;

			using var multi = await conn.QueryMultipleAsync(
				"sp_ListarCentrosCostoPag",
				new { p_offset = offset, p_limite = pageSize },
				commandType: CommandType.StoredProcedure
			);

			var data = await multi.ReadAsync<CentroCostoDto>();
			var total = await multi.ReadFirstAsync<int>();

			return (data, total);
		}

		public async Task<CentroCostoDto?> ObtenerAsync(string codigo)
		{
			using var conn = _db.CreateConnection();

			return await conn.QueryFirstOrDefaultAsync<CentroCostoDto>(
				"sp_ObtenerCentroCostoPorId",
				new { p_codigo = codigo },
				commandType: CommandType.StoredProcedure
			);
		}

		public async Task<int> CrearAsync(CentroCostoDto entity)
		{
			using var conn = _db.CreateConnection();

			return await conn.ExecuteAsync(
				"sp_InsertarCentroCosto",
				new
				{
					p_codigo = entity.Codigo,
					p_nombre = entity.Nombre,
					p_descripcion = entity.Descripcion,
					p_estado = entity.Estado
				},
				commandType: CommandType.StoredProcedure
			);
		}

		public async Task<int> ActualizarAsync(CentroCostoDto entity)
		{
			using var conn = _db.CreateConnection();

			return await conn.ExecuteAsync(
				"sp_ActualizarCentroCosto",
				new
				{
					p_codigo_busqueda = entity.Codigo,
					p_codigo_nuevo = entity.Codigo,
					p_nombre = entity.Nombre,
					p_descripcion = entity.Descripcion,
					p_estado = entity.Estado
				},
				commandType: CommandType.StoredProcedure
			);
		}

		public async Task<(bool, int)> EliminarAsync(string codigo)
		{
			using var conn = _db.CreateConnection();

			using var multi = await conn.QueryMultipleAsync(
				"sp_EliminarCentroCosto",
				new { p_codigo = codigo },
				commandType: CommandType.StoredProcedure
			);

			var tieneRelacion = await multi.ReadFirstAsync<bool>();
			var filas = await multi.ReadFirstAsync<int>();

			return (tieneRelacion, filas);
		}
	}
}
