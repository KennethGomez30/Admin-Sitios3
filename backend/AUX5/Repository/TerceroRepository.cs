using AUX5.Entities;
using Dapper;
using System.Data;

namespace AUX5.Repository
{
    public class TerceroRepository : ITerceroRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public TerceroRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        // Catálogos

        public async Task<IEnumerable<string>> ObtenerTiposAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            return await connection.QueryAsync<string>(
                "sp_terceros_tipos",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<string>> ObtenerEstadosAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            return await connection.QueryAsync<string>(
                "sp_terceros_estados",
                commandType: CommandType.StoredProcedure
            );
        }

        // CRUD

        public async Task<IEnumerable<TerceroEntity>> ListarAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            return await connection.QueryAsync<TerceroEntity>(
                "sp_terceros_listar",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<TerceroEntity?> ObtenerPorIdAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<TerceroEntity>(
                "sp_terceros_obtener_por_id",
                new { p_id = id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> ExisteIdentificacionAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var existe = await connection.ExecuteScalarAsync<int>(
                "sp_terceros_existe_identificacion",
                new { p_identificacion = identificacion },
                commandType: CommandType.StoredProcedure
            );

            return existe == 1;
        }

        // Inserta y retorna el registro completo tal como quedó en la BD
        public async Task<TerceroEntity> CrearAsync(TerceroEntity tercero, string usuario)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var parametros = new DynamicParameters();
            parametros.Add("p_identificacion", tercero.Identificacion);
            parametros.Add("p_nombre_razon_social", tercero.NombreRazonSocial);
            parametros.Add("p_tipo", tercero.Tipo);
            parametros.Add("p_email", tercero.Email);
            parametros.Add("p_telefono", tercero.Telefono);
            parametros.Add("p_estado", tercero.Estado);
            parametros.Add("p_usuario", usuario);
            parametros.Add("p_nuevo_id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_terceros_crear",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            var nuevoId = parametros.Get<int>("p_nuevo_id");

            return (await connection.QueryFirstOrDefaultAsync<TerceroEntity>(
                "sp_terceros_obtener_por_id",
                new { p_id = nuevoId },
                commandType: CommandType.StoredProcedure
            ))!;
        }

        // Actualiza y retorna el registro completo con fecha_modificacion
        // y usuario_modificacion refrescados desde la BD
        public async Task<TerceroEntity> ActualizarAsync(TerceroEntity tercero, string usuario)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_terceros_actualizar",
                new
                {
                    p_id = tercero.Id,
                    p_nombre_razon_social = tercero.NombreRazonSocial,
                    p_tipo = tercero.Tipo,
                    p_email = tercero.Email,
                    p_telefono = tercero.Telefono,
                    p_estado = tercero.Estado,
                    p_usuario = usuario
                },
                commandType: CommandType.StoredProcedure
            );

            return (await connection.QueryFirstOrDefaultAsync<TerceroEntity>(
                "sp_terceros_obtener_por_id",
                new { p_id = tercero.Id },
                commandType: CommandType.StoredProcedure
            ))!;
        }

        public async Task EliminarAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_terceros_eliminar",
                new { p_id = id },
                commandType: CommandType.StoredProcedure
            );
        }

        // Verifica relaciones usando SP
        public async Task<bool> TieneRelacionesAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var total = await connection.ExecuteScalarAsync<int>(
                "sp_terceros_tiene_relaciones",
                new { p_id = id },
                commandType: CommandType.StoredProcedure
            );

            return total > 0;
        }
    }
}