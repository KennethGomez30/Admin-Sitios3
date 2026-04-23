using System.Data;

namespace Bitacora.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}