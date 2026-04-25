using AUX12.Entities;

namespace AUX12.Services;

public interface IContactoService
{
    Task<BusinessLogicResponse> ListarPorTerceroAsync(int terceroId, string? usuario);
    Task<BusinessLogicResponse> ObtenerPorIdAsync(int id, string? usuario);
    Task<BusinessLogicResponse> CrearAsync(ContactoEntity c, string? usuario);
    Task<BusinessLogicResponse> ActualizarAsync(int id, ContactoEntity c, string? usuario);
    Task<BusinessLogicResponse> EliminarAsync(int id, string? usuario);
}