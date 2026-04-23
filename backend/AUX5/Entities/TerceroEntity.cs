namespace AUX5.Entities
{
    public class TerceroEntity
    {
        public int Id { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string NombreRazonSocial { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
    }

    // Requests

    public class CrearTerceroRequest
    {
        public string Identificacion { get; set; } = string.Empty;
        public string NombreRazonSocial { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string Estado { get; set; } = "Activo";
    }

    public class ActualizarTerceroRequest
    {
        public string NombreRazonSocial { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    // Responses

    public class BusinessLogicResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? ResponseObject { get; set; }
    }
}