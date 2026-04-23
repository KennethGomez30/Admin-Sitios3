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
                    return Results.Json(
                        new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." },
                        statusCode: 401);

                var result = await svc.ListarAsync(usuario);
                return result.StatusCode switch
                {
                    200 => Results.Ok(result),
                    _ => Results.Json(result, statusCode: 500)
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
                    return Results.Json(
                        new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." },
                        statusCode: 401);

                var result = await svc.ObtenerPorIdAsync(id, usuario);
                return result.StatusCode switch
                {
                    200 => Results.Ok(result),
                    404 => Results.Json(result, statusCode: 404),
                    _ => Results.Json(result, statusCode: 500)
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
                    return Results.Json(
                        new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." },
                        statusCode: 401);

                var result = await svc.CrearAsync(request, usuario);
                return result.StatusCode switch
                {
                    201 => Results.Json(result, statusCode: 201),
                    400 => Results.Json(result, statusCode: 400),
                    409 => Results.Json(result, statusCode: 409),
                    _ => Results.Json(result, statusCode: 500)
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
                    return Results.Json(
                        new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." },
                        statusCode: 401);

                var result = await svc.ActualizarAsync(id, request, usuario);
                return result.StatusCode switch
                {
                    200 => Results.Ok(result),
                    400 => Results.Json(result, statusCode: 400),
                    404 => Results.Json(result, statusCode: 404),
                    _ => Results.Json(result, statusCode: 500)
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
                    return Results.Json(
                        new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." },
                        statusCode: 401);

                var result = await svc.EliminarAsync(id, usuario);
                return result.StatusCode switch
                {
                    204 => Results.NoContent(),
                    404 => Results.Json(result, statusCode: 404),
                    409 => Results.Json(result, statusCode: 409),
                    _ => Results.Json(result, statusCode: 500)
                };
            })
            .WithName("EliminarTercero")
            .WithOpenApi();
        }
    }
}