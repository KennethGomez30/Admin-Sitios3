namespace PERMISOS.Entities
{
    public class UsuarioEntity
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int IntentosLogin { get; set; }
    }

    public class PantallaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
        public string MenuSeccion { get; set; } = string.Empty;
    }

    public class ActualizarIntentosRequest
    {
        public int Intentos { get; set; }
    }

    // Respuesta del endpoint GET /perfil que React consume directamente
    public class PerfilResponse
    {
        public string UsuarioNombre { get; set; } = string.Empty;
        public List<PantallaDto> Pantallas { get; set; } = [];
    }
}