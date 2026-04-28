using AUX6.Entities;

namespace AUX6.Repository
{
	public interface ICentroCostoRepository
	{
		Task<(IEnumerable<CentroCostoDto>, int)> ListarAsync(int pagina, int pageSize);

		Task<CentroCostoDto?> ObtenerAsync(string codigo);

		Task<int> CrearAsync(CentroCostoDto entity);

		Task<int> ActualizarAsync(CentroCostoDto entity);

		Task<(bool TieneRelacion, int Filas)> EliminarAsync(string codigo);
	}
}
