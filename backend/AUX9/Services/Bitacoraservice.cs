using System.Text;
using System.Text.Json;

namespace AUX9.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BitacoraService> _logger;
        private readonly IConfiguration _configuration;

        public BitacoraService(
            IHttpClientFactory httpClientFactory,
            ILogger<BitacoraService> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task RegistrarAsync(string descripcion, string? usuario = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Bitacora");
                var baseUrl = _configuration["BitacoraService:BaseUrl"]!;

                var payload = new
                {
                    Usuario = usuario,
                    Descripcion = descripcion
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync($"{baseUrl}/api/Bitacora", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Bitácora respondió con {StatusCode} para usuario {Usuario}.",
                        (int)response.StatusCode, usuario
                    );
                }
            }
            catch (Exception ex)
            {
                // La bitácora nunca debe interrumpir el flujo principal
                _logger.LogError(ex, "Error al llamar al microservicio de Bitácora.");
            }
        }
    }
}