using System.Data;

namespace AUX1.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}