namespace PERMISOS.Services
{
    public interface IAux1Service
    {
        Task<string?> ValidarTokenAsync(string? authorizationHeader);
    }
}