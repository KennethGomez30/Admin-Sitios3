using AUX1.Entities;
using AUX1.Services;
using Microsoft.AspNetCore.Mvc;

namespace AUX1
{
    public static class AutenticacionEndpoint
    {
        public static void MapAutenticacionEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/Autenticacion")
                .WithTags("Autenticacion")
                .RequireCors("AllowAll");

            // POST /api/Autenticacion/login
            group.MapPost("/login", async (
                [FromServices] IAutenticacionService svc,
                [FromBody] LoginRequest request) =>
            {
                var result = await svc.LoginAsync(request);
                return result.StatusCode switch
                {
                    200 => Results.Ok(result),
                    401 => Results.Json(result, statusCode: 401),
                    _ => Results.Json(result, statusCode: 500)
                };
            }).WithName("Login").WithOpenApi();

            // POST /api/Autenticacion/refresh
            group.MapPost("/refresh", async (
                [FromServices] IAutenticacionService svc,
                [FromBody] RefreshRequest request) =>
            {
                var result = await svc.RefreshAsync(request);
                return result.StatusCode switch
                {
                    200 => Results.Ok(result),
                    401 => Results.Json(result, statusCode: 401),
                    _ => Results.Json(result, statusCode: 500)
                };
            }).WithName("Refresh").WithOpenApi();

            // POST /api/Autenticacion/validate
            group.MapPost("/validate", async (
                [FromServices] IAutenticacionService svc,
                [FromBody] ValidateRequest request) =>
            {
                var valido = await svc.ValidateAsync(request);
                return valido
                    ? Results.NoContent()
                    : Results.Json(new { Message = "Token inválido." }, statusCode: 401);
            }).WithName("Validate").WithOpenApi();
 
            // POST /api/Autenticacion/logout
            group.MapPost("/logout", async (
                [FromServices] IAutenticacionService svc,
                [FromHeader(Name = "Authorization")] string? authorization) =>
            {
                var error = await svc.LogoutAsync(authorization ?? string.Empty);
                return error is null
                    ? Results.NoContent()
                    : Results.Json(error, statusCode: error.StatusCode);
            }).WithName("Logout").WithOpenApi();
        }
    }
}