using AUX1.Entities;

namespace AUX1.Repository
{
    public interface IAutenticacionRepository
    {
        // RefreshToken
        Task CrearRefreshTokenAsync(RefreshTokenEntity token);
        Task<RefreshTokenEntity?> ObtenerRefreshTokenAsync(string token);
        Task DesactivarRefreshTokenAsync(string token);
        Task DesactivarRefreshTokensUsuarioAsync(string usuarioEmail);
        Task ActualizarIntentosLoginAsync(string identificacion, int intentos);
        Task BloquearUsuarioAsync(string identificacion);
    }
}