using AUX7.Entities;
using AUX7.Services;
using Microsoft.AspNetCore.Mvc;

namespace AUX7
{
    public static class CentroCostoEndpoint
    {
        public static void MapCentroCostoEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/CentroCosto")
                .WithTags("CentroCosto")
                .RequireCors("AllowAll");

            // GET /api/CentroCosto/periodos
            group.MapGet("/periodos", async (
                [FromServices] ICentroCostoService svc,
                [FromServices] IAux1Service auth,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerPeriodosAsync();
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerPeriodos").WithOpenApi();

            // GET /api/CentroCosto/lineas?periodo_id={id}
            group.MapGet("/lineas", async (
                [FromServices] ICentroCostoService svc,
                [FromServices] IAux1Service auth,
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromQuery] int periodo_id) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerLineasAsync(periodo_id);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerLineas").WithOpenApi();

            // GET /api/CentroCosto/centros
            group.MapGet("/centros", async (
                [FromServices] ICentroCostoService svc,
                [FromServices] IAux1Service auth,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerCentrosActivosAsync();
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerCentros").WithOpenApi();

            // GET /api/CentroCosto/prorrateo/{detalleId}
            group.MapGet("/prorrateo/{detalleId:int}", async (
                [FromServices] ICentroCostoService svc,
                [FromServices] IAux1Service auth,
                [FromHeader(Name = "Authorization")] string? authorization,
                int detalleId) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerProrrateoAsync(detalleId);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerProrrateo").WithOpenApi();

            // POST /api/CentroCosto/prorrateo/agregar
            group.MapPost("/prorrateo/agregar", async (
                [FromServices] ICentroCostoService svc,
                [FromServices] IAux1Service auth,
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromBody] AgregarDistribucionRequest request) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                // usuario viene del JWT, no del header manual
                var result = await svc.AgregarDistribucionAsync(request, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("AgregarDistribucion").WithOpenApi();

            // POST /api/CentroCosto/prorrateo/eliminar
            group.MapPost("/prorrateo/eliminar", async (
                [FromServices] ICentroCostoService svc,
                [FromServices] IAux1Service auth,
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromBody] EliminarDistribucionRequest request) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.EliminarDistribucionAsync(request, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("EliminarDistribucion").WithOpenApi();
        }
    }
}