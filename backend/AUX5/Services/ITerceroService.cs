using AUX5.Entities;

namespace AUX5.Services
{
    public interface ITerceroService
    {
        Task<BusinessLogicResponse> ListarAsync(string usuario);
        Task<BusinessLogicResponse> ObtenerPorIdAsync(int id, string usuario);
        Task<BusinessLogicResponse> CrearAsync(CrearTerceroRequest request, string usuario);
        Task<BusinessLogicResponse> ActualizarAsync(int id, ActualizarTerceroRequest request, string usuario);
        Task<BusinessLogicResponse> EliminarAsync(int id, string usuario);
    }
}