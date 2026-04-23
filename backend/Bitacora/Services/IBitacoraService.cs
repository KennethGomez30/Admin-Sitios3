using Bitacora.Entities;

namespace Bitacora.Services
{
    public interface IBitacoraService
    {
        Task<BusinessLogicResponse> RegistrarAsync(BitacoraRequest request);
    }
}