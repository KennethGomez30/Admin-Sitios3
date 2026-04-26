namespace AUX9.Entities
{
    // ─── DTOs de base de datos ───────────────────────────────────────────────────

    public class TerceroDto
    {
        public int TerceroId { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class MovimientoTerceroDto
    {
        public int AsientoId { get; set; }
        public int DetalleId { get; set; }
        public string? Consecutivo { get; set; }
        public string? FechaAsiento { get; set; }
        public string? CodigoAsiento { get; set; }
        public string? Referencia { get; set; }
        public string CuentaCodigo { get; set; } = string.Empty;
        public string CuentaNombre { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public string EstadoCodigo { get; set; } = string.Empty;
        public string EstadoNombre { get; set; } = string.Empty;
    }

    // ─── Response paginado del reporte ──────────────────────────────────────────

    public class ReporteTercerosResponse
    {
        public IEnumerable<MovimientoTerceroDto> Movimientos { get; set; } = [];
        public decimal Total { get; set; }
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TotalPaginas { get; set; }
    }

    // ─── Wrapper de respuesta (mismo patrón que AUX1) ───────────────────────────

    public class BusinessLogicResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? ResponseObject { get; set; }
    }
}
