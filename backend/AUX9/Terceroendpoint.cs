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
            // Devuelve la lista de terceros para el select del filtro
            group.MapGet("/terceros", async (
                [FromServices] ITerceroService svc) =>
            {
                var result = await svc.ObtenerTercerosAsync();
                return result.StatusCode == 200
                    ? Results.Ok(result)
                    : Results.Json(result, statusCode: result.StatusCode);
            }).WithName("ObtenerTerceros").WithOpenApi();

            // GET /api/ReporteTerceros/movimientos
            // Filtros: tercero_id (obligatorio), fecha_inicio, fecha_fin, periodo_id, estado, pagina
            // Devuelve movimientos paginados (10 por página) + total
            group.MapGet("/movimientos", async (
                [FromServices] ITerceroService svc,
                HttpContext ctx,
                [FromQuery] int tercero_id,
                [FromQuery] DateTime? fecha_inicio,
                [FromQuery] DateTime? fecha_fin,
                [FromQuery] int? periodo_id,
                [FromQuery] string? estado,
                [FromQuery] int pagina = 1) =>
            {
                var usuario = ctx.Request.Headers["X-Usuario"].FirstOrDefault();

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
