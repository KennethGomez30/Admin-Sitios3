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
            // Devuelve todos los períodos contables disponibles
            group.MapGet("/periodos", async (
                [FromServices] ICentroCostoService svc) =>
            {
                var result = await svc.ObtenerPeriodosAsync();
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerPeriodos").WithOpenApi();

            // GET /api/CentroCosto/lineas?periodo_id={id}
            // Devuelve las líneas de asiento del período con su estado de distribución (index.php)
            group.MapGet("/lineas", async (
                [FromServices] ICentroCostoService svc,
                [FromQuery] int periodo_id) =>
            {
                var result = await svc.ObtenerLineasAsync(periodo_id);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerLineas").WithOpenApi();

            // GET /api/CentroCosto/centros
            // Devuelve todos los centros de costo activos
            group.MapGet("/centros", async (
                [FromServices] ICentroCostoService svc) =>
            {
                var result = await svc.ObtenerCentrosActivosAsync();
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerCentros").WithOpenApi();

            // GET /api/CentroCosto/prorrateo/{detalleId}
            // Devuelve el detalle del asiento + distribuciones registradas (prorrateo.php GET)
            group.MapGet("/prorrateo/{detalleId:int}", async (
                [FromServices] ICentroCostoService svc,
                int detalleId) =>
            {
                var result = await svc.ObtenerProrrateoAsync(detalleId);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerProrrateo").WithOpenApi();

            // POST /api/CentroCosto/prorrateo/agregar
            // Agrega una distribución de centro de costo a una línea (prorrateo.php accion=agregar)
            group.MapPost("/prorrateo/agregar", async (
                [FromServices] ICentroCostoService svc,
                [FromBody] AgregarDistribucionRequest request,
                HttpContext ctx) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.AgregarDistribucionAsync(request, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("AgregarDistribucion").WithOpenApi();

            // POST /api/CentroCosto/prorrateo/eliminar
            // Elimina una distribución de centro de costo (prorrateo.php accion=eliminar)
            group.MapPost("/prorrateo/eliminar", async (
                [FromServices] ICentroCostoService svc,
                [FromBody] EliminarDistribucionRequest request,
                HttpContext ctx) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();
                var result = await svc.EliminarDistribucionAsync(request, usuario);
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("EliminarDistribucion").WithOpenApi();
        }
    }
}