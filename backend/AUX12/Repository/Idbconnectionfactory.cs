using System.Data;

namespace AUX12.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}