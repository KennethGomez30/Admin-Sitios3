using AUX7.Entities;

namespace AUX7.Services
{
    public interface ICentroCostoService
    {
        Task<BusinessLogicResponse> ObtenerPeriodosAsync();
        Task<BusinessLogicResponse> ObtenerLineasAsync(int periodoId);
        Task<BusinessLogicResponse> ObtenerProrrateoAsync(int detalleId);
        Task<BusinessLogicResponse> ObtenerCentrosActivosAsync();
        Task<BusinessLogicResponse> AgregarDistribucionAsync(AgregarDistribucionRequest request, string? usuario);
        Task<BusinessLogicResponse> EliminarDistribucionAsync(EliminarDistribucionRequest request, string? usuario);
    }
}
