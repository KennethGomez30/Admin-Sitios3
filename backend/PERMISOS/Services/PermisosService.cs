using PERMISOS.Entities;
using PERMISOS.Repository;

namespace PERMISOS.Services
{
    public class PermisosService : IPermisosService
    {
        private readonly IPermisosRepository _repository;

        public PermisosService(IPermisosRepository repository)
        {
            _repository = repository;
        }

        public Task<UsuarioEntity?> ObtenerUsuarioPorIdentificacionAsync(string identificacion)
            => _repository.ObtenerUsuarioPorIdentificacionAsync(identificacion);

        public Task<List<string>> ObtenerRolesPorUsuarioAsync(string identificacion)
            => _repository.ObtenerRolesPorUsuarioAsync(identificacion);

        public Task<List<PantallaDto>> ObtenerPantallasPorRolesAsync(string rolesCsv)
            => _repository.ObtenerPantallasPorRolesAsync(rolesCsv);

        public async Task<PerfilResponse> ObtenerPerfilAsync(string identificacion)
        {
            // Obtener usuario y roles en paralelo son independientes entre sí
            var usuarioTask = _repository.ObtenerUsuarioPorIdentificacionAsync(identificacion);
            var rolesTask = _repository.ObtenerRolesPorUsuarioAsync(identificacion);

            await Task.WhenAll(usuarioTask, rolesTask);

            var usuario = await usuarioTask;
            var roles = await rolesTask;
            var rolesCsv = string.Join(",", roles);

            var pantallas = string.IsNullOrEmpty(rolesCsv)
                ? []
                : await _repository.ObtenerPantallasPorRolesAsync(rolesCsv);

            var nombreCompleto = usuario is not null
                ? $"{usuario.Nombre} {usuario.Apellido}".Trim()
                : identificacion;   // fallback mostrar identificacion si no hay nombre

            return new PerfilResponse
            {
                UsuarioNombre = nombreCompleto,
                Pantallas = pantallas
            };
        }
    }
}