using AUX1.Entities;
using System.Net.Http.Json;
using System.Text.Json;

namespace AUX1.Services
{
    public class PermisosService : IPermisosService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PermisosService> _logger;
        private readonly string _baseUrl;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PermisosService(
            IHttpClientFactory httpClientFactory,
            ILogger<PermisosService> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _baseUrl = configuration["PermisosService:BaseUrl"]!;
        }

        // GET /api/Permisos/usuarios/{identificacion}
        public async Task<UsuarioPermisoDto?> ObtenerUsuarioAsync(string identificacion)
        {
            var client = _httpClientFactory.CreateClient("Permisos");
            var response = await client.GetAsync(
                $"{_baseUrl}/api/Permisos/usuarios/{Uri.EscapeDataString(identificacion)}"
            );

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UsuarioPermisoDto>(_jsonOptions);
        }
    }
}