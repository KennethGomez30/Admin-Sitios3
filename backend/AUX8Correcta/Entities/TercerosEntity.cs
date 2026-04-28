namespace AUX8Correcta.Entities
{
    public class PeriodoContableDto
    {
        public int PeriodoId { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string? Estado { get; set; }
        public int Activo { get; set; }
    }

    public class LineaAsientoTerceroDto
    {
        public int DetalleId { get; set; }
        public int AsientoId { get; set; }
        public int CuentaId { get; set; }
        public string? CuentaCodigo { get; set; }
        public string? CuentaNombre { get; set; }
        public string? TipoMovimiento { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public string? EstadoCodigo { get; set; }
        public string? EstadoNombre { get; set; }
        public decimal MontoTerceros { get; set; }
        public int CantidadTerceros { get; set; }
    }

    public class DetalleAsientoTerceroDto
    {
        public int DetalleId { get; set; }
        public int AsientoId { get; set; }
        public int CuentaId { get; set; }
        public string? CuentaCodigo { get; set; }
        public string? CuentaNombre { get; set; }
        public string? TipoMovimiento { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public string? Consecutivo { get; set; }
        public DateTime FechaAsiento { get; set; }
        public string? Codigo { get; set; }
        public string? Referencia { get; set; }
        public string? EstadoCodigo { get; set; }
        public string? EstadoNombre { get; set; }
        public int PeriodoId { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
    }

    public class TerceroDto
    {
        public int Id { get; set; }
        public string? NombreRazonSocial { get; set; }
        public string? Identificacion { get; set; }
    }

    public class DistribucionTerceroDto
    {
        public int Id { get; set; }
        public int DetalleId { get; set; }
        public int TerceroId { get; set; }
        public decimal Monto { get; set; }
        public string? Nombre { get; set; }
        public string? Identificacion { get; set; }
    }

    public class AgregarDistribucionRequest
    {
        public int DetalleId { get; set; }
        public int TerceroId { get; set; }
        public decimal Monto { get; set; }
    }

    public class EliminarDistribucionRequest
    {
        public int Id { get; set; }
        public int DetalleId { get; set; }
    }
    public class BusinessLogicResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? ResponseObject { get; set; }
    }
}
