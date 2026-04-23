using AUX1.Entities;
using Dapper;
using System.Data;

namespace AUX1.Repository
{
    public class AutenticacionRepository : IAutenticacionRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public AutenticacionRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        // RefreshToken
        public async Task CrearRefreshTokenAsync(RefreshTokenEntity token)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_CrearRefreshToken",
                new
                {
                    p_Token = token.Token,
                    p_UsuarioEmail = token.UsuarioEmail,
                    p_FechaCreacion = token.FechaCreacion,
                    p_FechaExpiracion = token.FechaExpiracion,
                    p_Activo = token.Activo
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<RefreshTokenEntity?> ObtenerRefreshTokenAsync(string token)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<RefreshTokenEntity>(
                "sp_ObtenerRefreshToken",
                new { p_Token = token },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task DesactivarRefreshTokenAsync(string token)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_DesactivarRefreshToken",
                new { p_Token = token },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task DesactivarRefreshTokensUsuarioAsync(string usuarioEmail)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_DesactivarRefreshTokensUsuario",
                new { p_UsuarioEmail = usuarioEmail },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task ActualizarIntentosLoginAsync(string identificacion, int intentos)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_ActualizarIntentosLogin",
                new { p_Identificacion = identificacion, p_Intentos = intentos },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task BloquearUsuarioAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_BloquearUsuario",
                new { p_Identificacion = identificacion },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}