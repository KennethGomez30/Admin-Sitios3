using System.Data;

namespace AUX11.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}