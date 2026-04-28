namespace AUX10.Entities
{
	public class MovimientoCentroCostoDto
	{
		public int AsientoId { get; set; }
		public int DetalleId { get; set; }

		public string? FechaAsiento { get; set; }
		public string? CodigoAsiento { get; set; }

		public string CentroCostoCodigo { get; set; } = string.Empty;
		public string CentroCostoNombre { get; set; } = string.Empty;

		public string CuentaCodigo { get; set; } = string.Empty;
		public string CuentaNombre { get; set; } = string.Empty;

		public string TipoMovimiento { get; set; } = string.Empty;
		public decimal Monto { get; set; }

		public string EstadoCodigo { get; set; } = string.Empty;
		public string EstadoNombre { get; set; } = string.Empty;
	}

	public class ReporteCentroCostoResponse
	{
		public IEnumerable<MovimientoCentroCostoDto> Movimientos { get; set; } = [];
		public decimal Total { get; set; }
		public int TotalRegistros { get; set; }
		public int Pagina { get; set; }
		public int TotalPaginas { get; set; }
	}
	public class CentroCostoDto
	{
		public int CentroCostoId { get; set; }
		public string Codigo { get; set; } = string.Empty;
		public string Nombre { get; set; } = string.Empty;
	}

	public class BusinessLogicResponse
	{
		public int StatusCode { get; set; }
		public string Message { get; set; } = string.Empty;
		public object? ResponseObject { get; set; }
	}
}

