using System.Data;

namespace AUX5.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}