using AUX11.Entities;

namespace AUX11.Repository
{
    public interface IDireccionRepository
    {
        Task<IEnumerable<DireccionEntity>> ListarPorTerceroAsync(int terceroId);
        Task< DireccionEntity?> ObtenerPorIdAsync(int id);
        Task<int> CrearAsync(DireccionEntity d);
        Task<bool> ActualizarAsync(DireccionEntity d);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteTerceroAsync(int terceroId);
        Task<bool> QuitarPrincipalDeTerceroAsync(int terceroId, int? exceptoId = null);
    }
}
