using PERMISOS.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace PERMISOS.Services
{
    public class Aux1Service : IAux1Service
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Aux1Service> _logger;
        private readonly string _baseUrl;

        public Aux1Service(
            IHttpClientFactory httpClientFactory,
            ILogger<Aux1Service> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _baseUrl = configuration["AUX1Service:BaseUrl"]!;
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

                var payload = new { Token = token };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                // POST /api/Autenticacion/validate → 204 si válido, 401 si no
                var response = await client.PostAsync(
                    $"{_baseUrl}/api/Autenticacion/validate",
                    content
                );

                if (!response.IsSuccessStatusCode)
                    return null;

                // AUX1 confirmó la firma: leer el Subject del JWT es seguro sin ir a BD
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