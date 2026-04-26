namespace AUX12.Entities;

public class ContactoEntity
{
    public int Id { get; set; }
    public int TerceroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string Tipo { get; set; } = "Otro";       // 'Principal' | 'Facturación' | 'Cobros' | 'Soporte' | 'Otro'
    public string Estado { get; set; } = "Activo";   // 'Activo' | 'Inactivo'

    public DateTime FechaCreacion { get; set; }
    public string? UsuarioCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
}

// Wrapper de respuesta (mismo patrón que AUX11)
public class BusinessLogicResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? ResponseObject { get; set; }
}