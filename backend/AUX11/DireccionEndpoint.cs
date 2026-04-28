using AUX11.Entities;
using AUX11.Services;
using Microsoft.AspNetCore.Mvc;

namespace AUX11;

public static class DireccionEndpoint
{
    public static void MapDireccionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/TerceroDirecciones")
            .WithTags("TerceroDirecciones")
            .RequireCors("AllowAll");

        // GET /api/TerceroDirecciones/tercero/{terceroId}
        group.MapGet("/tercero/{terceroId:int}", async (
            [FromServices] IAux1Service auth,
            [FromServices] IDireccionService svc,
            [FromHeader(Name = "Authorization")] string? authorization,
            int terceroId) =>
        {
            var usuario = await auth.ValidarTokenAsync(authorization);
            if (usuario is null)
                return Results.Unauthorized();

            var direcciones = await svc.ListarPorTerceroAsync(terceroId, usuario);
            return Results.Ok(direcciones);
        })
        .WithName("ListarDireccionesPorTercero")
        .WithOpenApi();

        // GET /api/TerceroDirecciones/{id}
        group.MapGet("/{id:int}", async (
            [FromServices] IAux1Service auth,
            [FromServices] IDireccionService svc,
            [FromHeader(Name = "Authorization")] string? authorization,
            int id) =>
        {
            var usuario = await auth.ValidarTokenAsync(authorization);
            if (usuario is null)
                return Results.Unauthorized();

            var direccion = await svc.ObtenerPorIdAsync(id, usuario);
            return direccion is null ? Results.NotFound() : Results.Ok(direccion);
        })
        .WithName("ObtenerDireccionPorId")
        .WithOpenApi();

        // POST /api/TerceroDirecciones
        group.MapPost("/", async (
            [FromServices] IAux1Service auth,
            [FromServices] IDireccionService svc,
            [FromHeader(Name = "Authorization")] string? authorization,
            [FromBody] DireccionEntity direccion) =>
        {
            var usuario = await auth.ValidarTokenAsync(authorization);
            if (usuario is null)
                return Results.Unauthorized();

            var (creada, error) = await svc.CrearAsync(direccion, usuario);

            if (creada is null)
                return Results.BadRequest(new { message = error });

            return Results.Created($"/api/TerceroDirecciones/{creada.Id}", creada);
        })
        .WithName("CrearDireccion")
        .WithOpenApi();

        // PUT /api/TerceroDirecciones/{id}
        group.MapPut("/{id:int}", async (
            [FromServices] IAux1Service auth,
            [FromServices] IDireccionService svc,
            [FromHeader(Name = "Authorization")] string? authorization,
            int id,
            [FromBody] DireccionEntity direccion) =>
        {
            var usuario = await auth.ValidarTokenAsync(authorization);
            if (usuario is null)
                return Results.Unauthorized();

            if (id != direccion.Id && direccion.Id != 0)
                return Results.BadRequest(new { message = "El ID de la ruta y el del cuerpo no coinciden" });

            var (actualizada, error, notFound) = await svc.ActualizarAsync(id, direccion, usuario);

            if (notFound)
                return Results.NotFound();

            if (actualizada is null)
                return Results.BadRequest(new { message = error });

            return Results.Ok(actualizada);
        })
        .WithName("ActualizarDireccion")
        .WithOpenApi();

        // DELETE /api/TerceroDirecciones/{id}
        group.MapDelete("/{id:int}", async (
            [FromServices] IAux1Service auth,
            [FromServices] IDireccionService svc,
            [FromHeader(Name = "Authorization")] string? authorization,
            int id) =>
        {
            var usuario = await auth.ValidarTokenAsync(authorization);
            if (usuario is null)
                return Results.Unauthorized();

            var (ok, error, notFound) = await svc.EliminarAsync(id, usuario);

            if (notFound)
                return Results.NotFound();

            if (!ok)
                return Results.Conflict(new { message = error });

            return Results.NoContent();
        })
        .WithName("EliminarDireccion")
        .WithOpenApi();
    }
}