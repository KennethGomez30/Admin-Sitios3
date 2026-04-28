using AUX6.Repository;
using AUX6.Entities;

namespace AUX6.Services
{
	public class CentroCostoService : ICentroCostoService
	{
		private readonly ICentroCostoRepository _repo;
		private readonly IBitacoraService _bitacora;
		private readonly ILogger<CentroCostoService> _logger;

		private const int PageSize = 10;

		public CentroCostoService(
			ICentroCostoRepository repo,
			IBitacoraService bitacora,
			ILogger<CentroCostoService> logger)
		{
			_repo = repo;
			_bitacora = bitacora;
			_logger = logger;
		}

		public async Task<BusinessLogicResponse> ListarAsync(int pagina, string usuario)
		{
			var (data, total) = await _repo.ListarAsync(pagina, PageSize);

			_ = _bitacora.RegistrarAsync("El usuario consulta centros de costo", usuario);

			return new BusinessLogicResponse
			{
				StatusCode = 200,
				ResponseObject = new PagedCentroCostoResponse
				{
					Data = data,
					TotalRegistros = total,
					Pagina = pagina,
					TotalPaginas = (int)Math.Ceiling(total / (double)PageSize)
				}
			};
		}
		public async Task<BusinessLogicResponse> ObtenerAsync(string codigo, string usuario)
		{
			var data = await _repo.ObtenerAsync(codigo);

			if (data == null)
			{
				return new BusinessLogicResponse
				{
					StatusCode = 404,
					Message = "Centro de costo no encontrado"
				};
			}

			await _bitacora.RegistrarAsync(
				$"El usuario consulta centro de costo: {codigo}",
				usuario
			);

			return new BusinessLogicResponse
			{
				StatusCode = 200,
				ResponseObject = data
			};
		}

		public async Task<BusinessLogicResponse> CrearAsync(CentroCostoDto dto, string usuario)
		{
			await _repo.CrearAsync(dto);

			_ = _bitacora.RegistrarAsync(
				$"Se crea centro costo: {System.Text.Json.JsonSerializer.Serialize(dto)}",
				usuario);

			return new BusinessLogicResponse { StatusCode = 200, Message = "Creado correctamente" };
		}

		public async Task<BusinessLogicResponse> ActualizarAsync(CentroCostoDto dto, string usuario)
		{
			var anterior = await _repo.ObtenerAsync(dto.Codigo);

			await _repo.ActualizarAsync(dto);

			_ = _bitacora.RegistrarAsync(
				$"Actualiza centro costo: Antes={System.Text.Json.JsonSerializer.Serialize(anterior)} Después={System.Text.Json.JsonSerializer.Serialize(dto)}",
				usuario);

			return new BusinessLogicResponse { StatusCode = 200, Message = "Actualizado correctamente" };
		}

		public async Task<BusinessLogicResponse> EliminarAsync(string codigo, string usuario)
		{
			var (tieneRelacion, filas) = await _repo.EliminarAsync(codigo);

			if (tieneRelacion)
			{
				return new BusinessLogicResponse
				{
					StatusCode = 400,
					Message = "No se puede eliminar un registro con datos relacionados."
				};
			}

			_ = _bitacora.RegistrarAsync(
				$"Elimina centro costo: {codigo}",
				usuario);

			return new BusinessLogicResponse { StatusCode = 200, Message = "Eliminado correctamente" };
		}
	}
}
