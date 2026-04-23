using Dapper;
using PERMISOS.Entities;
using System.Data;

namespace PERMISOS.Repository
{
    public class PermisosRepository : IPermisosRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public PermisosRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<UsuarioEntity?> ObtenerUsuarioPorIdentificacionAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<UsuarioEntity>(
                "sp_ObtenerUsuarioPorIdentificacion",
                new { p_Identificacion = identificacion },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<List<string>> ObtenerRolesPorUsuarioAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var roles = await connection.QueryAsync<string>(
                "sp_ObtenerRolesPorUsuario",
                new { p_Identificacion = identificacion },
                commandType: CommandType.StoredProcedure
            );

            return roles.ToList();
        }

        public async Task<List<PantallaDto>> ObtenerPantallasPorRolesAsync(string rolesCsv)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var pantallas = await connection.QueryAsync<PantallaDto>(
                "sp_ObtenerMenuPorRolesREACT",
                new { p_roles_csv = rolesCsv },
                commandType: CommandType.StoredProcedure
            );

            return pantallas.ToList();
        }
    }
}