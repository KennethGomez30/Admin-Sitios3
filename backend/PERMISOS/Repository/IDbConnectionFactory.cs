using System.Data;

namespace PERMISOS.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}