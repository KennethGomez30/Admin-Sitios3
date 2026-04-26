using AUX12.Entities;
using AUX12.Services;
using Microsoft.AspNetCore.Mvc;

namespace AUX12
{
    public static class ContactoEndpoint
    {
        public static void MapContactoEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/TerceroContactos")
                .WithTags("TerceroContactos")
                .RequireCors("AllowAll");

            // GET /api/TerceroContactos/tercero/{terceroId}
            group.MapGet("/tercero/{terceroId:int}", async (
                [FromServices] IContactoService svc,
                HttpContext ctx,
                int terceroId) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.ListarPorTerceroAsync(terceroId, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ListarContactosPorTercero").WithOpenApi();

            // GET /api/TerceroContactos/{id}
            group.MapGet("/{id:int}", async (
                [FromServices] IContactoService svc,
                HttpContext ctx,
                int id) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.ObtenerPorIdAsync(id, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerContactoPorId").WithOpenApi();

            // POST /api/TerceroContactos
            group.MapPost("/", async (
                [FromServices] IContactoService svc,
                HttpContext ctx,
                [FromBody] ContactoEntity contacto) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.CrearAsync(contacto, usuario);
                return result.StatusCode == 201
                    ? Results.Created($"/api/TerceroContactos/{((ContactoEntity)result.ResponseObject!).Id}", result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("CrearContacto").WithOpenApi();

            // PUT /api/TerceroContactos/{id}
            group.MapPut("/{id:int}", async (
                [FromServices] IContactoService svc,
                HttpContext ctx,
                int id,
                [FromBody] ContactoEntity contacto) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.ActualizarAsync(id, contacto, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ActualizarContacto").WithOpenApi();

            // DELETE /api/TerceroContactos/{id}
            group.MapDelete("/{id:int}", async (
                [FromServices] IContactoService svc,
                HttpContext ctx,
                int id) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.EliminarAsync(id, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("EliminarContacto").WithOpenApi();
        }
    }
}