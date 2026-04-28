using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AUX10.Entities;
using AUX10.Services;

namespace AUX10
{
	public static class ReporteCentroCostoendpoint
	{
		public static void MapReporteCentroCostoEndpoints(this IEndpointRouteBuilder routes)
		{
			var group = routes
				.MapGroup("/api/ReporteCentroCosto")
				.RequireCors("AllowAll");

			group.MapGet("/movimientos", async (
				[FromServices] IReporteService svc,
				[FromServices] IAux1Service auth,
				[FromHeader(Name = "Authorization")] string? authorization,

				[FromQuery] string? centro_costo,
				[FromQuery] DateTime? fecha_inicio,
				[FromQuery] DateTime? fecha_fin,
				[FromQuery] string? periodoId,
				[FromQuery] string? estado,
				[FromQuery] int pagina = 1
			) =>
			{
				var usuario = await auth.ValidarTokenAsync(authorization);

				if (usuario is null)
					return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

				var result = await svc.ObtenerMovimientosAsync(
					centro_costo,
					fecha_inicio,
					fecha_fin,
					periodoId,
					estado,
					pagina,
					usuario
				);

				return result.StatusCode == 200
					? Results.Ok(result)
					: Results.Json(result, statusCode: result.StatusCode);

			}).WithName("ObtenerReporteCentroCosto")
			  .WithOpenApi();

			group.MapGet("/centro-costo", async (
				[FromServices] IReporteService svc,
				[FromServices] IAux1Service auth,
				[FromHeader(Name = "Authorization")] string? authorization
			) =>
			{
				var usuario = await auth.ValidarTokenAsync(authorization);
				if (usuario is null)
					return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

				var result = await svc.ListarCentrosCostoAsync(usuario);
				return Results.Ok(result);
			})
			.WithName("ListarCentrosCostoReporte")
			.WithOpenApi();
		}

	}
}
