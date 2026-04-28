using Dapper;
using AUX11.Entities;

namespace AUX11.Repository
{
    public class DireccionRepository : IDireccionRepository
    {
        private readonly IDbConnectionFactory _factory;

        public DireccionRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<DireccionEntity>> ListarPorTerceroAsync(int terceroId)
        {
            using var con = _factory.CreateConnection();
            var sql = @"SELECT id, tercero_id, alias, ubicacion, direccion_exacta,
                           es_principal, estado,
                           fecha_creacion, usuario_creacion,
                           fecha_modificacion, usuario_modificacion
                    FROM tercero_direcciones
                    WHERE tercero_id = @terceroId
                    ORDER BY es_principal DESC, id DESC";
            return await con.QueryAsync<DireccionEntity>(sql, new { terceroId });
        }

        public async Task<DireccionEntity?> ObtenerPorIdAsync(int id)
        {
            using var con = _factory.CreateConnection();
            var sql = @"SELECT id, tercero_id, alias, ubicacion, direccion_exacta,
                           es_principal, estado,
                           fecha_creacion, usuario_creacion,
                           fecha_modificacion, usuario_modificacion
                    FROM tercero_direcciones
                    WHERE id = @id";
            return await con.QueryFirstOrDefaultAsync<DireccionEntity>(sql, new { id });
        }

        public async Task<int> CrearAsync(DireccionEntity d)
        {
            using var con = _factory.CreateConnection();
            var sql = @"INSERT INTO tercero_direcciones
                    (tercero_id, alias, ubicacion, direccion_exacta,
                     es_principal, estado, usuario_creacion)
                    VALUES
                    (@TerceroId, @Alias, @Ubicacion, @DireccionExacta,
                     @EsPrincipal, @Estado, @UsuarioCreacion);
                    SELECT LAST_INSERT_ID();";
            return await con.ExecuteScalarAsync<int>(sql, d);
        }

        public async Task<bool> ActualizarAsync(DireccionEntity d)
        {
            using var con = _factory.CreateConnection();
            var sql = @"UPDATE tercero_direcciones
                    SET alias = @Alias,
                        ubicacion = @Ubicacion,
                        direccion_exacta = @DireccionExacta,
                        es_principal = @EsPrincipal,
                        estado = @Estado,
                        usuario_modificacion = @UsuarioModificacion
                    WHERE id = @Id";
            var rows = await con.ExecuteAsync(sql, d);
            return rows > 0;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            using var con = _factory.CreateConnection();
            var sql = "DELETE FROM tercero_direcciones WHERE id = @id";
            var rows = await con.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        public async Task<bool> ExisteTerceroAsync(int terceroId)
        {
            using var con = _factory.CreateConnection();
            var sql = "SELECT COUNT(1) FROM terceros WHERE id = @terceroId";
            var count = await con.ExecuteScalarAsync<int>(sql, new { terceroId });
            return count > 0;
        }

        public async Task<bool> QuitarPrincipalDeTerceroAsync(int terceroId, int? exceptoId = null)
        {
            using var con = _factory.CreateConnection();
            var sql = exceptoId.HasValue
                ? @"UPDATE tercero_direcciones
                SET es_principal = 0
                WHERE tercero_id = @terceroId AND id <> @exceptoId"
                : @"UPDATE tercero_direcciones
                SET es_principal = 0
                WHERE tercero_id = @terceroId";

            await con.ExecuteAsync(sql, new { terceroId, exceptoId });
            return true;
        }
    }
}
