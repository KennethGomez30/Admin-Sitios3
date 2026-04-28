using AUX8Correcta.Entities;

namespace AUX8Correcta.Services
{
    public interface ITercerosservice
    {
        Task<BusinessLogicResponse> ObtenerPeriodosAsync();
        Task<BusinessLogicResponse> ObtenerLineasAsync(int periodoId);
        Task<BusinessLogicResponse> ObtenerProrrateoAsync(int detalleId);
        Task<BusinessLogicResponse> ObtenerTercerosActivosAsync();
        Task<BusinessLogicResponse> AgregarDistribucionAsync(AgregarDistribucionRequest request, string? usuario);
        Task<BusinessLogicResponse> EliminarDistribucionAsync(EliminarDistribucionRequest request, string? usuario);
    }
}
