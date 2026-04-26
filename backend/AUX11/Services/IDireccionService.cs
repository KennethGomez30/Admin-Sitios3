namespace AUX11.Entities
{
    public interface IDireccionService
    {
        Task<BusinessLogicResponse> ListarPorTerceroAsync(int terceroId, string? usuario);
        Task<BusinessLogicResponse> ObtenerPorIdAsync(int id, string? usuario);
        Task<BusinessLogicResponse> CrearAsync(DireccionEntity d, string? usuario);
        Task<BusinessLogicResponse> ActualizarAsync(int id, DireccionEntity d, string? usuario);
        Task<BusinessLogicResponse> EliminarAsync(int id, string? usuario);
    }
}
