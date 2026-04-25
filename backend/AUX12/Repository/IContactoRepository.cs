using AUX12.Entities;

namespace AUX12.Repository;

public interface IContactoRepository
{
    Task<IEnumerable<ContactoEntity>> ListarPorTerceroAsync(int terceroId);
    Task<ContactoEntity?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(ContactoEntity c);
    Task<bool> ActualizarAsync(ContactoEntity c);
    Task<bool> EliminarAsync(int id);
    Task<bool> ExisteTerceroAsync(int terceroId);
}