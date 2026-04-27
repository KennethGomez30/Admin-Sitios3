using AUX8Correcta.Entities;
using AUX8Correcta.Services;
using Microsoft.AspNetCore.Mvc;

namespace AUX8Correcta.Endpoints
{
    public static class TercerosEndpoint
    {
        public static void MapTercerosEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/Terceros")
                           .WithTags("Terceros")
                           .RequireCors("AllowAll");

            // GET /api/Terceros/periodos
            group.MapGet("/periodos", async (
                [FromServices] IAux1Service auth,
                [FromServices] ITercerosservice svc,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerPeriodosAsync();
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("Terceros_Periodos")
            .WithOpenApi();

            // GET /api/Terceros/lineas/{periodoId}
            group.MapGet("/lineas/{periodoId:int}", async (
                int periodoId,
                [FromServices] IAux1Service auth,
                [FromServices] ITercerosservice svc,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerLineasAsync(periodoId);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("Terceros_Lineas")
            .WithOpenApi();

            // GET /api/Terceros/prorrateo/{detalleId}
            group.MapGet("/prorrateo/{detalleId:int}", async (
                int detalleId,
                [FromServices] IAux1Service auth,
                [FromServices] ITercerosservice svc,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerProrrateoAsync(detalleId);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("Terceros_Prorrateo")
            .WithOpenApi();

            // GET /api/Terceros/activos
            group.MapGet("/activos", async (
                [FromServices] IAux1Service auth,
                [FromServices] ITercerosservice svc,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerTercerosActivosAsync();
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("Terceros_Activos")
            .WithOpenApi();

            // POST /api/Terceros/distribuciones
            group.MapPost("/distribuciones", async (
                [FromServices] IAux1Service auth,
                [FromServices] ITercerosservice svc,
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromBody] AgregarDistribucionRequest request) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." }, statusCode: 401);

                var result = await svc.AgregarDistribucionAsync(request, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("Terceros_AgregarDistribucion")
            .WithOpenApi();

            // DELETE /api/Terceros/distribuciones/{id}?detalleId=23
            group.MapDelete("/distribuciones/{id:int}", async (
                int id,
                [FromQuery] int detalleId,
                [FromServices] IAux1Service auth,
                [FromServices] ITercerosservice svc,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." }, statusCode: 401);

                var request = new EliminarDistribucionRequest
                {
                    Id = id,
                    DetalleId = detalleId
                };

                var result = await svc.EliminarDistribucionAsync(request, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("Terceros_EliminarDistribucion")
            .WithOpenApi();
        }
    }
}