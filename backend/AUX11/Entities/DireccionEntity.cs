namespace AUX11.Entities
{
    public class DireccionEntity
    {
        public int Id { get; set; }
        public int TerceroId { get; set; }
        public string Alias { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public string DireccionExacta { get; set; } = string.Empty;
        public bool EsPrincipal { get; set; }
        public string Estado { get; set; } = "Activa"; // 'Activa' | 'Inactiva'

        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
    }

    // ─── Wrapper de respuesta (mismo patrón que AUX1) ───────────────────────────

    public class BusinessLogicResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? ResponseObject { get; set; }
    }
}
