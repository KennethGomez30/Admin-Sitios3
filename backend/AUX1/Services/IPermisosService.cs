using AUX1.Entities;

namespace AUX1.Services
{
    public interface IPermisosService
    {
        Task<UsuarioPermisoDto?> ObtenerUsuarioAsync(string identificacion);
    }
}