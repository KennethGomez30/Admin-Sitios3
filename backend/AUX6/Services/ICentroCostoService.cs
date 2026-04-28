using AUX6.Entities;

namespace AUX6.Services
{
	public interface ICentroCostoService
	{
		Task<BusinessLogicResponse> ListarAsync(int pagina, string usuario);
		Task<BusinessLogicResponse> ObtenerAsync(string codigo, string usuario);
		Task<BusinessLogicResponse> CrearAsync(CentroCostoDto dto, string usuario);

		Task<BusinessLogicResponse> ActualizarAsync(CentroCostoDto dto, string usuario);

		Task<BusinessLogicResponse> EliminarAsync(string codigo, string usuario);
	}
}
