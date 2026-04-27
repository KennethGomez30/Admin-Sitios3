using AUX12.Entities;
using AUX12.Services;
using Microsoft.AspNetCore.Mvc;

namespace AUX12;

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
            [FromServices] IAux1Service auth,
            [FromServices] IContactoService svc,
            [FromHeader(Name = "Authorization")] string? authorization,
            int terceroId) =>
        {
            var usuario = await auth.ValidarTokenAsync(authorization);
            if (usuario is null)
                return Results.Unauthorized();

            var contactos = await svc.ListarPorTerceroAsync(terceroId, usuario);
            return Results.Ok(contactos);
        })
        .WithName("ListarContactosPorTercero")
        .WithOpenApi();

        // GET /api/TerceroContactos/{id}
        group.MapGet("/{id:int}", async (
            [FromServices] IAux1Service auth,
            [FromServices] IContactoService svc,
            [FromHeader(Name = "Authorization")] string? authorization,
            int id) =>
        {
            var usuario = await auth.ValidarTokenAsync(authorization);
            if (usuario is null)
                return Results.Unauthorized();

            var contacto = await svc.ObtenerPorIdAsync(id, usuario);
            return contacto is null ? Results.NotFound() : Results.Ok(contacto);
        })
        .WithName("ObtenerContactoPorId")
        .WithOpenApi();

        // POST /api/TerceroContactos
        group.MapPost("/", async (
            [FromServices] IAux1Service auth,
            [FromServices] IContactoService svc,
            [FromHeader(Name = "Authorization")] string? authorization,
            [FromBody] ContactoEntity contacto) =>
        {
            var usuario = await auth.ValidarTokenAsync(authorization);
            if (usuario is null)
                return Results.Unauthorized();

            var (creado, error) = await svc.CrearAsync(contacto, usuario);

            if (creado is null)
                return Results.BadRequest(new { message = error });

            return Results.Created($"/api/TerceroContactos/{creado.Id}", creado);
        })
        .WithName("CrearContacto")
        .WithOpenApi();

        // PUT /api/TerceroContactos/{id}
        group.MapPut("/{id:int}", async (
            [FromServices] IAux1Service auth,
            [FromServices] IContactoService svc,
            [FromHeader(Name = "Authorization")] string? authorization,
            int id,
            [FromBody] ContactoEntity contacto) =>
        {
            var usuario = await auth.ValidarTokenAsync(authorization);
            if (usuario is null)
                return Results.Unauthorized();

            if (id != contacto.Id && contacto.Id != 0)
                return Results.BadRequest(new { message = "El ID de la ruta y el del cuerpo no coinciden" });

            var (actualizado, error, notFound) = await svc.ActualizarAsync(id, contacto, usuario);

            if (notFound)
                return Results.NotFound();

            if (actualizado is null)
                return Results.BadRequest(new { message = error });

            return Results.Ok(actualizado);
        })
        .WithName("ActualizarContacto")
        .WithOpenApi();

        // DELETE /api/TerceroContactos/{id}
        group.MapDelete("/{id:int}", async (
            [FromServices] IAux1Service auth,
            [FromServices] IContactoService svc,
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
        .WithName("EliminarContacto")
        .WithOpenApi();
    }
}