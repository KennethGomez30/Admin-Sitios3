using Bitacora.Entities;

namespace Bitacora.Repository
{
    public interface IBitacoraRepository
    {
        Task Registrar(BitacoraEntity bitacora);
    }
}