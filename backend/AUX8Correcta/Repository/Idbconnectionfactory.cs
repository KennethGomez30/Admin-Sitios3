using System.Data;

namespace AUX8Correcta.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}