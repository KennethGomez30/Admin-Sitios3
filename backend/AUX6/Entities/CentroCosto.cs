namespace AUX6.Entities
{
	public class CentroCostoDto
	{
		public int CentroCostoId { get; set; }
		public string Codigo { get; set; } = string.Empty;
		public string Nombre { get; set; } = string.Empty;
		public string? Descripcion { get; set; }
		public string Estado { get; set; } = string.Empty; // Activo / Inactivo
	}

	public class PagedCentroCostoResponse
	{
		public IEnumerable<CentroCostoDto> Data { get; set; } = [];
		public int TotalRegistros { get; set; }
		public int Pagina { get; set; }
		public int TotalPaginas { get; set; }
	}

	public class BusinessLogicResponse
	{
		public int StatusCode { get; set; }
		public string Message { get; set; } = string.Empty;
		public object? ResponseObject { get; set; }
	}
}
