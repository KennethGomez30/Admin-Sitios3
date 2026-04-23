namespace AUX1.Entities
{
    public class UsuarioPermisoDto
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int IntentosLogin { get; set; }
    }

    // Entidad propia de AUX1
    public class RefreshTokenEntity
    {
        public string Token { get; set; } = string.Empty;
        public string UsuarioEmail { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Activo { get; set; }
    }

    // Requests contratos de entrada desde React
    public class LoginRequest
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ValidateRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresIn { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
    }

    public class RefreshResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresIn { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
    }

    // Sobre que envuelve todas las respuestas de AUX1 hacia React
    public class BusinessLogicResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? ResponseObject { get; set; }
    }
}