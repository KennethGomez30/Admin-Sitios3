namespace AUX1.Services
{
    public interface IJwtService
    {
        string GenerarJwtToken(string identificacion, int expiracionMinutos);
        string GenerarRefreshToken();
        bool ValidarToken(string token);
    }
}