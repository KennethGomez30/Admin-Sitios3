using Bitacora.Entities;
using Bitacora.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bitacora
{
    public static class BitacoraEndpoint
    {
        public static void MapBitacoraEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/Bitacora")
                .WithTags("Bitacora")
                .RequireCors("AllowAll");

            // POST /api/Bitacora
            group.MapPost("/", async (
                [FromServices] IBitacoraService bitacoraService,
                [FromBody] BitacoraRequest request) =>
            {
                var result = await bitacoraService.RegistrarAsync(request);
                return result.StatusCode switch
                {
                    204 => Results.NoContent(),
                    400 => Results.BadRequest(new { result.Message }),
                    _ => Results.Problem(result.Message)
                };
            })
            .WithName("RegistrarBitacora")
            .WithOpenApi();
        }
    }
}