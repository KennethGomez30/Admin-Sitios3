using PERMISOS.Entities;

namespace PERMISOS.Services
{
    public interface IPermisosService
    {
        Task<UsuarioEntity?> ObtenerUsuarioPorIdentificacionAsync(string identificacion);
        Task<List<string>> ObtenerRolesPorUsuarioAsync(string identificacion);
        Task<List<PantallaDto>> ObtenerPantallasPorRolesAsync(string rolesCsv);

        // Compone el perfil completo para React con el nombre del usuario + pantallaS
        Task<PerfilResponse> ObtenerPerfilAsync(string identificacion);
    }
}