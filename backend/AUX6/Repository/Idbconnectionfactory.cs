using System.Data;

namespace AUX6.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}