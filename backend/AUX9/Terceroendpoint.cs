using AUX9.Services;
using Microsoft.AspNetCore.Mvc;

namespace AUX9
{
    public static class TerceroEndpoint
    {
        public static void MapTerceroEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/ReporteTerceros")
                .WithTags("ReporteTerceros")
                .RequireCors("AllowAll");

            // GET /api/ReporteTerceros/terceros
            group.MapGet("/terceros", async (
                [FromServices] ITerceroService svc,
                [FromServices] IAux1Service auth,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerTercerosAsync();
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerTerceros").WithOpenApi();

            // GET /api/ReporteTerceros/movimientos
            group.MapGet("/movimientos", async (
                [FromServices] ITerceroService svc,
                [FromServices] IAux1Service auth,
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromQuery] int tercero_id,
                [FromQuery] DateTime? fecha_inicio,
                [FromQuery] DateTime? fecha_fin,
                [FromQuery] int? periodo_id,
                [FromQuery] string? estado,
                [FromQuery] int pagina = 1) =>
            {
                var usuario = await auth.ValidarTokenAsync(authorization);
                if (usuario is null)
                    return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

                var result = await svc.ObtenerMovimientosAsync(
                    tercero_id,
                    fecha_inicio,
                    fecha_fin,
                    periodo_id,
                    estado,
                    pagina,
                    usuario
                );

                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerMovimientos").WithOpenApi();
        }
    }
}