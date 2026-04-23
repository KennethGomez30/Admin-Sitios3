using System.Data;

namespace AUX9.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}