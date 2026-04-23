namespace AUX7.Services
{
    public interface IBitacoraService
    {
        Task RegistrarAsync(string descripcion, string? usuario = null);
    }
}