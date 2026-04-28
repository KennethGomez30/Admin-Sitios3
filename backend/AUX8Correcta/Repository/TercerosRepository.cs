using AUX8Correcta.Entities;
using Dapper;
using Microsoft.AspNetCore.Connections;
using System.Data;

namespace AUX8Correcta.Repository
{
    public class TercerosRepository : Itercerosrepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public TercerosRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<PeriodoContableDto>> ObtenerPeriodosAsync()
        {
            const string sql = @"
                SELECT
                    periodo_id AS PeriodoId,
                    anio AS Anio,
                    mes AS Mes,
                    estado AS Estado,
                    activo AS Activo
                FROM periodos_contables
                ORDER BY anio DESC, mes DESC;";

            using var conn = _dbConnectionFactory.CreateConnection();
            return await conn.QueryAsync<PeriodoContableDto>(sql);
        }

        public async Task<IEnumerable<LineaAsientoTerceroDto>> ObtenerLineasAsientoPorPeriodoAsync(int periodoId)
        {
            const string sql = @"
                SELECT
                    ad.detalle_id AS DetalleId,
                    ad.asiento_id AS AsientoId,
                    ad.cuenta_id AS CuentaId,
                    cc.codigo AS CuentaCodigo,
                    cc.nombre AS CuentaNombre,
                    ad.tipo_movimiento AS TipoMovimiento,
                    ad.monto AS Monto,
                    ad.descripcion AS Descripcion,
                    a.estado_codigo AS EstadoCodigo,
                    COALESCE(e.nombre, a.estado_codigo) AS EstadoNombre,
                    COALESCE(SUM(adt.monto),0) AS MontoTerceros,
                    COUNT(adt.id) AS CantidadTerceros
                FROM asiento_detalle ad
                INNER JOIN asientos a ON a.asiento_id = ad.asiento_id
                INNER JOIN cuenta_contable cc ON cc.id_cuenta = ad.cuenta_id
                LEFT JOIN estados_asiento e ON e.codigo = a.estado_codigo
                LEFT JOIN asiento_detalle_tercero adt ON adt.detalle_id = ad.detalle_id
                WHERE a.periodo_id = @PeriodoId
                GROUP BY
                    ad.detalle_id, ad.asiento_id, ad.cuenta_id, cc.codigo, cc.nombre,
                    ad.tipo_movimiento, ad.monto, ad.descripcion, a.estado_codigo, e.nombre
                ORDER BY ad.asiento_id, ad.detalle_id;";

            using var conn = _dbConnectionFactory.CreateConnection();
            return await conn.QueryAsync<LineaAsientoTerceroDto>(sql, new { PeriodoId = periodoId });
        }

        public async Task<DetalleAsientoTerceroDto?> ObtenerDetalleAsientoAsync(int detalleId)
        {
            const string sql = @"
                SELECT
                    ad.detalle_id AS DetalleId,
                    ad.asiento_id AS AsientoId,
                    ad.cuenta_id AS CuentaId,
                    cc.codigo AS CuentaCodigo,
                    cc.nombre AS CuentaNombre,
                    ad.tipo_movimiento AS TipoMovimiento,
                    ad.monto AS Monto,
                    ad.descripcion AS Descripcion,
                    a.consecutivo AS Consecutivo,
                    a.fecha_asiento AS FechaAsiento,
                    a.codigo AS Codigo,
                    a.referencia AS Referencia,
                    a.estado_codigo AS EstadoCodigo,
                    COALESCE(e.nombre, a.estado_codigo) AS EstadoNombre,
                    p.periodo_id AS PeriodoId,
                    p.anio AS Anio,
                    p.mes AS Mes
                FROM asiento_detalle ad
                INNER JOIN asientos a ON a.asiento_id = ad.asiento_id
                INNER JOIN periodos_contables p ON p.periodo_id = a.periodo_id
                INNER JOIN cuenta_contable cc ON cc.id_cuenta = ad.cuenta_id
                LEFT JOIN estados_asiento e ON e.codigo = a.estado_codigo
                WHERE ad.detalle_id = @DetalleId
                LIMIT 1;";

            using var conn = _dbConnectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<DetalleAsientoTerceroDto>(sql, new { DetalleId = detalleId });
        }

        public async Task<IEnumerable<TerceroDto>> ObtenerTercerosActivosAsync()
        {
            const string sql = @"
                SELECT
                    id AS Id,
                    nombre_razon_social AS NombreRazonSocial,
                    identificacion AS Identificacion
                FROM terceros
                WHERE estado = 'Activo'
                ORDER BY nombre_razon_social;";

            using var conn = _dbConnectionFactory.CreateConnection();
            return await conn.QueryAsync<TerceroDto>(sql);
        }

        public async Task<IEnumerable<DistribucionTerceroDto>> ObtenerDistribucionesAsync(int detalleId)
        {
            const string sql = @"
                SELECT
                    adt.id AS Id,
                    adt.detalle_id AS DetalleId,
                    adt.tercero_id AS TerceroId,
                    adt.monto AS Monto,
                    t.nombre_razon_social AS Nombre,
                    t.identificacion AS Identificacion
                FROM asiento_detalle_tercero adt
                INNER JOIN terceros t ON t.id = adt.tercero_id
                WHERE adt.detalle_id = @DetalleId
                ORDER BY t.nombre_razon_social;";

            using var conn = _dbConnectionFactory.CreateConnection();
            return await conn.QueryAsync<DistribucionTerceroDto>(sql, new { DetalleId = detalleId });
        }

        public async Task<DistribucionTerceroDto?> ObtenerDistribucionPorIdAsync(int id, int detalleId)
        {
            const string sql = @"
                SELECT
                    adt.id AS Id,
                    adt.detalle_id AS DetalleId,
                    adt.tercero_id AS TerceroId,
                    adt.monto AS Monto,
                    t.nombre_razon_social AS Nombre,
                    t.identificacion AS Identificacion
                FROM asiento_detalle_tercero adt
                INNER JOIN terceros t ON t.id = adt.tercero_id
                WHERE adt.id = @Id AND adt.detalle_id = @DetalleId
                LIMIT 1;";

            using var conn = _dbConnectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<DistribucionTerceroDto>(sql, new { Id = id, DetalleId = detalleId });
        }

        public async Task<bool> ExisteDistribucionAsync(int detalleId, int terceroId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM asiento_detalle_tercero
                WHERE detalle_id = @DetalleId AND tercero_id = @TerceroId;";

            using var conn = _dbConnectionFactory.CreateConnection();
            var total = await conn.ExecuteScalarAsync<int>(sql, new { DetalleId = detalleId, TerceroId = terceroId });
            return total > 0;
        }

        public async Task AgregarDistribucionAsync(int detalleId, int terceroId, decimal monto, string? usuario)
        {
            const string sql = @"
                INSERT INTO asiento_detalle_tercero
                (detalle_id, tercero_id, monto, usuario_creacion, fecha_creacion)
                VALUES (@DetalleId, @TerceroId, @Monto, @Usuario, NOW());";

            using var conn = _dbConnectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, new
            {
                DetalleId = detalleId,
                TerceroId = terceroId,
                Monto = monto,
                Usuario = usuario
            });
        }

        public async Task EliminarDistribucionAsync(int id, int detalleId)
        {
            const string sql = @"
                DELETE FROM asiento_detalle_tercero
                WHERE id = @Id AND detalle_id = @DetalleId;";

            using var conn = _dbConnectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, new { Id = id, DetalleId = detalleId });
        }


    }
}
