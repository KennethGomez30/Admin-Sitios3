using AUX9.Entities;
using AUX9.Repository;

namespace AUX9.Services
{
    public class TerceroService : ITerceroService
    {
        private readonly ITerceroRepository _repository;
        private readonly IBitacoraService _bitacoraService;
        private readonly ILogger<TerceroService> _logger;

        private const int ElementosPorPagina = 10;

        public TerceroService(
            ITerceroRepository repository,
            IBitacoraService bitacoraService,
            ILogger<TerceroService> logger)
        {
            _repository = repository;
            _bitacoraService = bitacoraService;
            _logger = logger;
        }

        // GET terceros (para llenar el select del filtro)
        public async Task<BusinessLogicResponse> ObtenerTercerosAsync()
        {
            try
            {
                var terceros = await _repository.ObtenerTercerosAsync();
                return new BusinessLogicResponse { StatusCode = 200, Message = "OK", ResponseObject = terceros };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener terceros.");
                return new BusinessLogicResponse { StatusCode = 500, Message = "Error interno al obtener terceros." };
            }
        }

        // GET reporte de movimientos paginado (AUX9 index.php)
        public async Task<BusinessLogicResponse> ObtenerMovimientosAsync(
            int terceroId,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? periodoId,
            string? estadoCodigo,
            int pagina,
            string? usuario)
        {
            try
            {
                if (terceroId <= 0)
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Debe seleccionar un tercero válido."
                    };

                if (pagina < 1) pagina = 1;

                var (movimientos, totalRegistros) = await _repository.ObtenerMovimientosPorTerceroAsync(
                    terceroId,
                    fechaInicio,
                    fechaFin,
                    periodoId,
                    estadoCodigo,
                    pagina,
                    ElementosPorPagina
                );

                var lista = movimientos.ToList();
                var totalMonto = lista.Sum(m => m.Monto);
                var totalPaginas = totalRegistros == 0
                    ? 1
                    : (int)Math.Ceiling(totalRegistros / (double)ElementosPorPagina);

                _ = _bitacoraService.RegistrarAsync(
                    "El usuario consulta reporte de movimientos por tercero",
                    usuario
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "OK",
                    ResponseObject = new ReporteTercerosResponse
                    {
                        Movimientos = lista,
                        Total = totalMonto,
                        TotalRegistros = totalRegistros,
                        Pagina = pagina,
                        TotalPaginas = totalPaginas
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de movimientos para tercero {TerceroId}.", terceroId);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Error interno al generar el reporte."
                };
            }
        }
    }
}