using System.Data;
using AUX7.Entities;
using Dapper;
using Microsoft.AspNetCore.Connections;

namespace AUX7.Repository
{
    public class CentroCostoRepository : ICentroCostoRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public CentroCostoRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<PeriodoContableDto>> ObtenerPeriodosAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<PeriodoContableDto>(
                "sp_ObtenerPeriodosContables",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<LineaAsientoDto>> ObtenerLineasAsientoPorPeriodoAsync(int periodoId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<LineaAsientoDto>(
                "sp_ObtenerLineasAsientoPorPeriodo",
                new { p_PeriodoId = periodoId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<DetalleAsientoDto?> ObtenerDetalleAsientoAsync(int detalleId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<DetalleAsientoDto>(
                "sp_ObtenerDetalleAsiento",
                new { p_DetalleId = detalleId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<CentroCostoDto>> ObtenerCentrosActivosAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<CentroCostoDto>(
                "sp_ObtenerCentrosActivos",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<DistribucionCentroCostoDto>> ObtenerDistribucionesAsync(int detalleId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<DistribucionCentroCostoDto>(
                "sp_ObtenerDistribucionesPorDetalle",
                new { p_DetalleId = detalleId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> ExisteDistribucionAsync(int detalleId, int centroCostoId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "sp_ExisteDistribucionCentroCosto",
                new { p_DetalleId = detalleId, p_CentroCostoId = centroCostoId },
                commandType: CommandType.StoredProcedure
            );
            return count > 0;
        }

        public async Task<bool> CentroCostoExisteYActivoAsync(int centroCostoId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "sp_CentroCostoExisteYActivo",
                new { p_CentroCostoId = centroCostoId },
                commandType: CommandType.StoredProcedure
            );
            return count > 0;
        }

        public async Task AgregarDistribucionAsync(int detalleId, int centroCostoId, decimal monto, string? usuario)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "sp_AgregarDistribucionCentroCosto",
                new
                {
                    p_DetalleId = detalleId,
                    p_CentroCostoId = centroCostoId,
                    p_Monto = monto,
                    p_Usuario = usuario
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task EliminarDistribucionAsync(int id, int detalleId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "sp_EliminarDistribucionCentroCosto",
                new { p_Id = id, p_DetalleId = detalleId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<DistribucionCentroCostoDto?> ObtenerDistribucionPorIdAsync(int id, int detalleId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<DistribucionCentroCostoDto>(
                "sp_ObtenerDistribucionPorId",
                new { p_Id = id, p_DetalleId = detalleId },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
