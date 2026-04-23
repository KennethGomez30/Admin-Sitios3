namespace AUX1.Services
{
    public interface IBitacoraService
    {
        Task RegistrarAsync(string descripcion, string? usuario = null);
    }
}