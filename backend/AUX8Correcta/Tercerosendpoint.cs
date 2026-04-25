using AUX8Correcta.Entities;
using AUX8Correcta.Services;
using Microsoft.AspNetCore.Mvc;

namespace AUX8Correcta.Endpoints
{
    public static class Tercerosendpoint
    {
        public static void MapTercerosEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/terceros")
                           .WithTags("Terceros");

            // GET periodos
            group.MapGet("/periodos", async (ITercerosservice service) =>
            {
                var result = await service.ObtenerPeriodosAsync();
                return Results.Json(result, statusCode: result.StatusCode);
            });

            // GET lineas por periodo
            group.MapGet("/lineas/{periodoId:int}", async (int periodoId, ITercerosservice service) =>
            {
                var result = await service.ObtenerLineasAsync(periodoId);
                return Results.Json(result, statusCode: result.StatusCode);
            });

            // GET detalle + distribuciones
            group.MapGet("/prorrateo/{detalleId:int}", async (int detalleId, ITercerosservice service) =>
            {
                var result = await service.ObtenerProrrateoAsync(detalleId);
                return Results.Json(result, statusCode: result.StatusCode);
            });

            // GET terceros activos
            group.MapGet("/activos", async (ITercerosservice service) =>
            {
                var result = await service.ObtenerTercerosActivosAsync();
                return Results.Json(result, statusCode: result.StatusCode);
            });

            // POST agregar distribución
            group.MapPost("/distribuciones", async (AgregarDistribucionRequest request, ITercerosservice service) =>
            {
                var result = await service.AgregarDistribucionAsync(request, null);
                return Results.Json(result, statusCode: result.StatusCode);
            });

            // DELETE eliminar distribución
            group.MapDelete("/distribuciones/{id:int}", async (int id, int detalleId, ITercerosservice service) =>
            {
                var request = new EliminarDistribucionRequest
                {
                    Id = id,
                    DetalleId = detalleId
                };

                var result = await service.EliminarDistribucionAsync(request, null);
                return Results.Json(result, statusCode: result.StatusCode);
            });
        }

    }
}