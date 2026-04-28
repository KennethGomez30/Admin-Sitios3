using System.Data;
using MySql.Data.MySqlClient;

namespace AUX8Correcta.Repository
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }
    }
}