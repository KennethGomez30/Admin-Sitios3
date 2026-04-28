using System.Data;

namespace AUX10.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}