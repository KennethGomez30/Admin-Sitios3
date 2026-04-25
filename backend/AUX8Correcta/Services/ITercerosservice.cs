using AUX8Correcta.Entities;

namespace AUX8Correcta.Services
{
    public interface ITercerosservice
    {
        // GET periodos
        Task<BusinessLogicResponse> ObtenerPeriodosAsync();

        // GET lineas por periodo
        Task<BusinessLogicResponse> ObtenerLineasAsync(int periodoId);

        // GET detalle + distribuciones
        Task<BusinessLogicResponse> ObtenerProrrateoAsync(int detalleId);

        // GET terceros activos
        Task<BusinessLogicResponse> ObtenerTercerosActivosAsync();

        // POST agregar distribución
        Task<BusinessLogicResponse> AgregarDistribucionAsync(AgregarDistribucionRequest request, string? usuario);

        // DELETE eliminar distribución
        Task<BusinessLogicResponse> EliminarDistribucionAsync(EliminarDistribucionRequest request, string? usuario);
    }
}
