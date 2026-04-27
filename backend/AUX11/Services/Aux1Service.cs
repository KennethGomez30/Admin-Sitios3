using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace AUX11.Services
{
    public class Aux1Service : IAux1Service
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Aux1Service> _logger;
        private readonly IConfiguration _configuration;

        public Aux1Service(
            IHttpClientFactory httpClientFactory,
            ILogger<Aux1Service> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string?> ValidarTokenAsync(string? authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader) ||
                !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            var token = authorizationHeader["Bearer ".Length..].Trim();

            try
            {
                var client = _httpClientFactory.CreateClient("AUX1");
                var baseUrl = _configuration["AUX1Service:BaseUrl"]!;

                var payload = new { Token = token };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                // POST /api/Autenticacion/validate DA 204 si es válido 401 si no
                var response = await client.PostAsync($"{baseUrl}/api/Autenticacion/validate", content);

                if (!response.IsSuccessStatusCode)
                    return null;

                // Extraer el sub osea identificación del JWT sin ir a BD, ya que AUX1 lo firmó
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return jwt.Subject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar token contra AUX1.");
                return null;
            }
        }
    }
}