using AUX5.Entities;

namespace AUX5.Repository
{
    public interface ITerceroRepository
    {
        Task<IEnumerable<string>> ObtenerTiposAsync();
        Task<IEnumerable<string>> ObtenerEstadosAsync();
        Task<IEnumerable<TerceroEntity>> ListarAsync();
        Task<TerceroEntity?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteIdentificacionAsync(string identificacion);
        Task<TerceroEntity> CrearAsync(TerceroEntity tercero, string usuario); // retorna entidad completa
        Task<TerceroEntity> ActualizarAsync(TerceroEntity tercero, string usuario); // retorna entidad completa
        Task EliminarAsync(int id);
        Task<bool> TieneRelacionesAsync(int id);
    }
}