using Microsoft.AspNetCore.Mvc;
using PERMISOS.Entities;
using PERMISOS.Services;

namespace PERMISOS
{
    public static class PermisosEndpoint
    {
        public static void MapPermisosEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/Permisos")
                .WithTags("Permisos")
                .RequireCors("AllowAll");

            // GET /api/Permisos/perfil
            group.MapGet("/perfil", async (
                [FromServices] IPermisosService svc,
                [FromServices] IAux1Service aux1,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var identificacion = await aux1.ValidarTokenAsync(authorization);

                if (identificacion is null)
                    return Results.Json(
                        new { Message = "No autorizado." },
                        statusCode: 401
                    );

                var perfil = await svc.ObtenerPerfilAsync(identificacion);
                return Results.Ok(perfil);

            }).WithName("ObtenerPerfil").WithOpenApi();

            // GET /api/Permisos/usuarios/{identificacion}
            // Consumido únicamente por AUX1 para verificar credenciales en login.
            group.MapGet("/usuarios/{identificacion}", async (
                [FromServices] IPermisosService svc,
                string identificacion) =>
            {
                var usuario = await svc.ObtenerUsuarioPorIdentificacionAsync(identificacion);
                return usuario is null
                    ? Results.NotFound()
                    : Results.Ok(usuario);
            }).WithName("ObtenerUsuario").WithOpenApi();

            // GET /api/Permisos/usuarios/{identificacion}/roles
            // devuelve los roles del usuario como array de strings.
            group.MapGet("/usuarios/{identificacion}/roles", async (
                [FromServices] IPermisosService svc,
                string identificacion) =>
            {
                var roles = await svc.ObtenerRolesPorUsuarioAsync(identificacion);
                return Results.Ok(roles);
            }).WithName("ObtenerRoles").WithOpenApi();

            // GET /api/Permisos/pantallas?rolesCsv={csv}
            group.MapGet("/pantallas", async (
                [FromServices] IPermisosService svc,
                [FromQuery] string rolesCsv) =>
            {
                if (string.IsNullOrWhiteSpace(rolesCsv))
                    return Results.Ok(new List<PantallaDto>());

                var pantallas = await svc.ObtenerPantallasPorRolesAsync(rolesCsv);
                return Results.Ok(pantallas);
            }).WithName("ObtenerPantallas").WithOpenApi();
        }
    }
}