using PERMISOS.Entities;

namespace PERMISOS.Repository
{
    public interface IPermisosRepository
    {
        Task<UsuarioEntity?> ObtenerUsuarioPorIdentificacionAsync(string identificacion);
        Task<List<string>> ObtenerRolesPorUsuarioAsync(string identificacion);
        Task<List<PantallaDto>> ObtenerPantallasPorRolesAsync(string rolesCsv);
    }
}