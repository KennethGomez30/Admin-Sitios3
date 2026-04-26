using System.Data;
using AUX9.Entities;
using Dapper;
using Microsoft.AspNetCore.Connections;

namespace AUX9.Repository
{
    public class TerceroRepository : ITerceroRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public TerceroRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<TerceroDto>> ObtenerTercerosAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<TerceroDto>(
                "sp_ObtenerTerceros",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<(IEnumerable<MovimientoTerceroDto> Movimientos, int Total)> ObtenerMovimientosPorTerceroAsync(
            int terceroId,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? periodoId,
            string? estadoCodigo,
            int pagina,
            int elementosPorPagina)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var offset = (pagina - 1) * elementosPorPagina;

            // El SP devuelve dos result sets:
            //   1. Los movimientos paginados
            //   2. El total de registros (para calcular páginas)
            using var multi = await connection.QueryMultipleAsync(
                "sp_ObtenerMovimientosPorTercero",
                new
                {
                    p_TerceroId = terceroId,
                    p_FechaInicio = fechaInicio,
                    p_FechaFin = fechaFin,
                    p_PeriodoId = periodoId,
                    p_EstadoCodigo = estadoCodigo,
                    p_Offset = offset,
                    p_Limit = elementosPorPagina
                },
                commandType: CommandType.StoredProcedure
            );

            var movimientos = await multi.ReadAsync<MovimientoTerceroDto>();
            var total = await multi.ReadFirstOrDefaultAsync<int>();

            return (movimientos, total);
        }
    }
}
