namespace AUX7.Entities
{
    // ─── DTOs de base de datos ───────────────────────────────────────────────────

    public class PeriodoContableDto
    {
        public int PeriodoId { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int Activo { get; set; }
    }

    public class LineaAsientoDto
    {
        public int DetalleId { get; set; }
        public int AsientoId { get; set; }
        public int CuentaId { get; set; }
        public string CuentaCodigo { get; set; } = string.Empty;
        public string CuentaNombre { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public string EstadoCodigo { get; set; } = string.Empty;
        public string EstadoNombre { get; set; } = string.Empty;
        public decimal MontoProrrateado { get; set; }
        public int CantidadCentros { get; set; }
    }

    public class DetalleAsientoDto
    {
        public int DetalleId { get; set; }
        public int AsientoId { get; set; }
        public int CuentaId { get; set; }
        public string CuentaCodigo { get; set; } = string.Empty;
        public string CuentaNombre { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public string? Consecutivo { get; set; }
        public string? FechaAsiento { get; set; }
        public string? Codigo { get; set; }
        public string? Referencia { get; set; }
        public string EstadoCodigo { get; set; } = string.Empty;
        public string EstadoNombre { get; set; } = string.Empty;
        public int PeriodoId { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
    }

    public class DistribucionCentroCostoDto
    {
        public int Id { get; set; }
        public int DetalleId { get; set; }
        public int CentroCostoId { get; set; }
        public decimal Monto { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class CentroCostoDto
    {
        public int CentroCostoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    // ─── Requests (contratos de entrada) ────────────────────────────────────────

    public class AgregarDistribucionRequest
    {
        public int DetalleId { get; set; }
        public int CentroCostoId { get; set; }
        public decimal Monto { get; set; }
    }

    public class EliminarDistribucionRequest
    {
        public int Id { get; set; }
        public int DetalleId { get; set; }
    }

    // ─── Wrapper de respuesta (mismo patrón que AUX1) ───────────────────────────

    public class BusinessLogicResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? ResponseObject { get; set; }
    }
}