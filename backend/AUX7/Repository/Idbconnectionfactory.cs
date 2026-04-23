using System.Data;

namespace AUX7.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}