namespace AUX5.Services
{
    public interface IAux1Service
    {
        Task<string?> ValidarTokenAsync(string? authorizationHeader);
    }
}