using AUX12.Entities;

namespace AUX12.Services;

public interface IContactoService
{
    Task<IEnumerable<ContactoEntity>> ListarPorTerceroAsync(int terceroId, string? usuario);
    Task<ContactoEntity?> ObtenerPorIdAsync(int id, string? usuario);
    Task<(ContactoEntity? contacto, string? error)> CrearAsync(ContactoEntity contacto, string? usuario);
    Task<(ContactoEntity? contacto, string? error, bool notFound)> ActualizarAsync(int id, ContactoEntity contacto, string? usuario);
    Task<(bool ok, string? error, bool notFound)> EliminarAsync(int id, string? usuario);
}