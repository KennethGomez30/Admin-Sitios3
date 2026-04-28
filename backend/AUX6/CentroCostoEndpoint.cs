using AUX6.Entities;
using AUX6.Services;
using Microsoft.AspNetCore.Mvc;


namespace AUX6
{
	public static class CentroCostoEndpoint
	{
		public static void MapCentroCostoEndpoints(this IEndpointRouteBuilder routes)
		{
			var group = routes.MapGroup("/api/CentroCosto")
				.WithTags("CentroCosto")
				.RequireCors("AllowAll");

			group.MapGet("/", async (
				[FromServices] ICentroCostoService svc,
				[FromServices] IAux1Service auth,
				[FromHeader(Name = "Authorization")] string? authorization,
				[FromQuery] int pagina = 1
			) =>
			{
				var usuario = await auth.ValidarTokenAsync(authorization);
				if (usuario is null)
					return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

				var result = await svc.ListarAsync(pagina, usuario);

				return result.StatusCode == 200
					? Results.Ok(result)
					: Results.Json(result, statusCode: result.StatusCode);
			})
			.WithName("ListarCentroCosto");


			group.MapGet("/{codigo}", async (
				[FromServices] ICentroCostoService svc,
				[FromServices] IAux1Service auth,
				[FromHeader(Name = "Authorization")] string? authorization,
				string codigo
			) =>
			{
				var usuario = await auth.ValidarTokenAsync(authorization);
				if (usuario is null)
					return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

				var result = await svc.ObtenerAsync(codigo, usuario);

				return result.StatusCode == 200
					? Results.Ok(result)
					: Results.Json(result, statusCode: result.StatusCode);
			})
			.WithName("ObtenerCentroCosto");




			group.MapPost("/", async (
				[FromServices] ICentroCostoService svc,
				[FromServices] IAux1Service auth,
				[FromHeader(Name = "Authorization")] string? authorization,
				[FromBody] CentroCostoDto model
			) =>
			{
				var usuario = await auth.ValidarTokenAsync(authorization);
				if (usuario is null)
					return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

				var result = await svc.CrearAsync(model, usuario);

				return result.StatusCode == 200
					? Results.Ok(result)
					: Results.Json(result, statusCode: result.StatusCode);
			})
			.WithName("CrearCentroCosto");




			group.MapPut("/{codigo}", async (
				[FromServices] ICentroCostoService svc,
				[FromServices] IAux1Service auth,
				[FromHeader(Name = "Authorization")] string? authorization,
				string codigo,
				[FromBody] CentroCostoDto model
			) =>
			{
				var usuario = await auth.ValidarTokenAsync(authorization);
				if (usuario is null)
					return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

				model.Codigo = codigo;

				var result = await svc.ActualizarAsync(model, usuario);

				return result.StatusCode == 200
					? Results.Ok(result)
					: Results.Json(result, statusCode: result.StatusCode);
			})
			.WithName("ActualizarCentroCosto");



			group.MapDelete("/{codigo}", async (
				[FromServices] ICentroCostoService svc,
				[FromServices] IAux1Service auth,
				[FromHeader(Name = "Authorization")] string? authorization,
				string codigo
			) =>
			{
				var usuario = await auth.ValidarTokenAsync(authorization);
				if (usuario is null)
					return Results.Json(new { Message = "No autorizado." }, statusCode: 401);

				var result = await svc.EliminarAsync(codigo, usuario);

				return result.StatusCode == 200
					? Results.Ok(result)
					: Results.Json(result, statusCode: result.StatusCode);
			})
			.WithName("EliminarCentroCosto");

		}
	}
}

