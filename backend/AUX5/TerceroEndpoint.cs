using AUX5.Entities;
using AUX5.Services;
using Microsoft.AspNetCore.Mvc;

namespace AUX5
{
    public static class TerceroEndpoint
    {
        public static void MapTercerosEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/Terceros")
                .WithTags("Terceros")
                .RequireCors("AllowAll");

            // GET /api/Terceros
            group.MapGet("/", async (
                [FromServices] IAux1Service auth,
                [FromServices] ITerceroService svc,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ListarAsync(usuario);
                return result.StatusCode switch
                {
                    200 => Results.Ok(result.ResponseObject),
                    _ => Results.Problem(result.Message)
                };
            })
            .WithName("ListarTerceros")
            .WithOpenApi();

            // GET /api/Terceros/{id}
            group.MapGet("/{id:int}", async (
                [FromServices] IAux1Service auth,
                [FromServices] ITerceroService svc,
                [FromHeader(Name = "Authorization")] string? authorization,
                int id) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerPorIdAsync(id, usuario);
                return result.StatusCode switch
                {
                    200 => Results.Ok(result.ResponseObject),
                    404 => Results.NotFound(new { result.Message }),
                    _ => Results.Problem(result.Message)
                };
            })
            .WithName("ObtenerTercero")
            .WithOpenApi();

            // POST /api/Terceros
            group.MapPost("/", async (
                [FromServices] IAux1Service auth,
                [FromServices] ITerceroService svc,
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromBody] CrearTerceroRequest request) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.CrearAsync(request, usuario);
                return result.StatusCode switch
                {
                    201 => Results.Created(
                               $"/api/Terceros/{((TerceroEntity)result.ResponseObject!).Id}",
                               result.ResponseObject),
                    400 => Results.BadRequest(new { result.Message }),
                    409 => Results.Conflict(new { result.Message }),
                    _ => Results.Problem(result.Message)
                };
            })
            .WithName("CrearTercero")
            .WithOpenApi();

            // PUT /api/Terceros/{id}
            group.MapPut("/{id:int}", async (
                [FromServices] IAux1Service auth,
                [FromServices] ITerceroService svc,
                [FromHeader(Name = "Authorization")] string? authorization,
                int id,
                [FromBody] ActualizarTerceroRequest request) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ActualizarAsync(id, request, usuario);
                return result.StatusCode switch
                {
                    200 => Results.Ok(result.ResponseObject),
                    400 => Results.BadRequest(new { result.Message }),
                    404 => Results.NotFound(new { result.Message }),
                    _ => Results.Problem(result.Message)
                };
            })
            .WithName("ActualizarTercero")
            .WithOpenApi();

            // DELETE /api/Terceros/{id}
            group.MapDelete("/{id:int}", async (
                [FromServices] IAux1Service auth,
                [FromServices] ITerceroService svc,
                [FromHeader(Name = "Authorization")] string? authorization,
                int id) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.EliminarAsync(id, usuario);
                return result.StatusCode switch
                {
                    204 => Results.NoContent(),
                    404 => Results.NotFound(new { result.Message }),
                    409 => Results.Conflict(new { result.Message }),
                    _ => Results.Problem(result.Message)
                };
            })
            .WithName("EliminarTercero")
            .WithOpenApi();
        }
    }
}