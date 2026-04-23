using Bitacora.Entities;
using Dapper;

namespace Bitacora.Repository
{
    public class BitacoraRepository : IBitacoraRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public BitacoraRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task Registrar(BitacoraEntity bitacora)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_RegistrarBitacora",
                new
                {
                    p_FechaBitacora = bitacora.FechaBitacora,
                    p_Usuario = bitacora.Usuario,
                    p_Descripcion = bitacora.Descripcion
                },
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}