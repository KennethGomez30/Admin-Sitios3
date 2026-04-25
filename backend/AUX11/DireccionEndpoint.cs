using AUX11.Entities;
using Microsoft.AspNetCore.Mvc;
namespace AUX11
{
    public static class DireccionEndpoint
    {
        public static void MapDireccionEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/TerceroDirecciones")
                .WithTags("TerceroDirecciones")
                .RequireCors("AllowAll");

            // GET /api/TerceroDirecciones/tercero/{terceroId}
            // Lista todas las direcciones de un tercero (sin paginación)
            group.MapGet("/tercero/{terceroId:int}", async (
                [FromServices] IDireccionService svc,
                HttpContext ctx,
                int terceroId) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.ListarPorTerceroAsync(terceroId, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ListarDireccionesPorTercero").WithOpenApi();

            // GET /api/TerceroDirecciones/{id}
            // Obtiene una dirección por id (para precargar el formulario de edición)
            group.MapGet("/{id:int}", async (
                [FromServices] IDireccionService svc,
                HttpContext ctx,
                int id) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.ObtenerPorIdAsync(id, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerDireccionPorId").WithOpenApi();

            // POST /api/TerceroDirecciones
            // Crea una nueva dirección
            group.MapPost("/", async (
                [FromServices] IDireccionService svc,
                HttpContext ctx,
                [FromBody] DireccionEntity direccion) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.CrearAsync(direccion, usuario);
                return result.StatusCode == 201
                    ? Results.Created($"/api/TerceroDirecciones/{((DireccionEntity)result.ResponseObject!).Id}", result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("CrearDireccion").WithOpenApi();

            // PUT /api/TerceroDirecciones/{id}
            // Actualiza una dirección existente
            group.MapPut("/{id:int}", async (
                [FromServices] IDireccionService svc,
                HttpContext ctx,
                int id,
                [FromBody] DireccionEntity direccion) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.ActualizarAsync(id, direccion, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ActualizarDireccion").WithOpenApi();

            // DELETE /api/TerceroDirecciones/{id}
            // Elimina una dirección
            group.MapDelete("/{id:int}", async (
                [FromServices] IDireccionService svc,
                HttpContext ctx,
                int id) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.EliminarAsync(id, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("EliminarDireccion").WithOpenApi();
        }
    }
}
