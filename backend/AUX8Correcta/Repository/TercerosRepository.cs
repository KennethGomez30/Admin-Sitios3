using System.Data;
using AUX8Correcta.Entities;
using Dapper;

namespace AUX8Correcta.Repository
{
    public class TercerosRepository : Itercerosrepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public TercerosRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
    }
}
