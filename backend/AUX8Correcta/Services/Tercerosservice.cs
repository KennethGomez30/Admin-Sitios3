using AUX8Correcta.Entities;
using AUX8Correcta.Repository;
using Microsoft.AspNetCore.Mvc;

namespace AUX8Correcta.Services
{
    public class TercerosService : ITercerosservice
    {
        private readonly Itercerosrepository _repository;
        private readonly ILogger<TercerosService> _logger;

        // Estados que permiten asignar terceros: EA3 = Borrador, EA4 = Pendiente de aprobación
        private static readonly string[] _estadosPermitidos = ["EA3", "EA4"];

        public TercerosService(
            Itercerosrepository repository,
            ILogger<TercerosService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET periodos
        public async Task<BusinessLogicResponse> ObtenerPeriodosAsync()
        {
            try
            {
                var periodos = await _repository.ObtenerPeriodosAsync();
                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "OK",
                    ResponseObject = periodos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener períodos contables.");
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Error interno al obtener períodos."
                };
            }
        }

        // GET lineas por periodo
        public async Task<BusinessLogicResponse> ObtenerLineasAsync(int periodoId)
        {
            try
            {
                var lineas = await _repository.ObtenerLineasAsientoPorPeriodoAsync(periodoId);

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "OK",
                    ResponseObject = lineas
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener líneas de asiento para período {PeriodoId}.", periodoId);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Error interno al obtener líneas de asiento."
                };
            }
        }

        // GET detalle + distribuciones
        public async Task<BusinessLogicResponse> ObtenerProrrateoAsync(int detalleId)
        {
            try
            {
                var detalle = await _repository.ObtenerDetalleAsientoAsync(detalleId);

                if (detalle is null)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No se encontró la línea de asiento seleccionada."
                    };
                }

                var estadoActual = (detalle.EstadoCodigo ?? string.Empty).ToUpper().Trim();
                if (!_estadosPermitidos.Contains(estadoActual))
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 403,
                        Message = "No se permite prorratear terceros para este asiento."
                    };
                }

                var terceros = await _repository.ObtenerTercerosActivosAsync();
                var distribuciones = await _repository.ObtenerDistribucionesAsync(detalleId);
                var montoDistribuido = distribuciones.Sum(d => d.Monto);

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "OK",
                    ResponseObject = new
                    {
                        Detalle = detalle,
                        TercerosActivos = terceros,
                        Distribuciones = distribuciones,
                        MontoDistribuido = montoDistribuido,
                        Pendiente = detalle.Monto - montoDistribuido
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener prorrateo del detalle {DetalleId}.", detalleId);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Error interno al obtener información de prorrateo."
                };
            }
        }

        // GET terceros activos
        public async Task<BusinessLogicResponse> ObtenerTercerosActivosAsync()
        {
            try
            {
                var terceros = await _repository.ObtenerTercerosActivosAsync();
                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "OK",
                    ResponseObject = terceros
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener terceros activos.");
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Error interno al obtener terceros."
                };
            }
        }

        // POST agregar distribución
        public async Task<BusinessLogicResponse> AgregarDistribucionAsync(AgregarDistribucionRequest request, string? usuario)
        {
            try
            {
                if (request.DetalleId <= 0)
                    return new BusinessLogicResponse { StatusCode = 400, Message = "Detalle de asiento no válido." };

                if (request.TerceroId <= 0)
                    return new BusinessLogicResponse { StatusCode = 400, Message = "Debe seleccionar un tercero." };

                if (request.Monto <= 0)
                    return new BusinessLogicResponse { StatusCode = 400, Message = "El monto debe ser mayor a cero." };

                var detalle = await _repository.ObtenerDetalleAsientoAsync(request.DetalleId);
                if (detalle is null)
                    return new BusinessLogicResponse { StatusCode = 404, Message = "No se encontró la línea de asiento seleccionada." };

                var estadoActual = (detalle.EstadoCodigo ?? string.Empty).ToUpper().Trim();
                if (!_estadosPermitidos.Contains(estadoActual))
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 403,
                        Message = "No se puede modificar el prorrateo porque el asiento no está en estado permitido."
                    };
                }

                var terceroValido = (await _repository.ObtenerTercerosActivosAsync())
                    .Any(t => t.Id == request.TerceroId);

                if (!terceroValido)
                    return new BusinessLogicResponse { StatusCode = 400, Message = "El tercero seleccionado no existe o no está activo." };

                var distribuciones = await _repository.ObtenerDistribucionesAsync(request.DetalleId);
                var montoDistribuido = distribuciones.Sum(d => d.Monto);

                if (montoDistribuido + request.Monto - detalle.Monto > 0.009m)
                    return new BusinessLogicResponse { StatusCode = 400, Message = "La distribución excede el monto total de la línea." };

                var yaExiste = await _repository.ExisteDistribucionAsync(request.DetalleId, request.TerceroId);
                if (yaExiste)
                    return new BusinessLogicResponse { StatusCode = 400, Message = "Ese tercero ya fue agregado a esta línea." };

                await _repository.AgregarDistribucionAsync(request.DetalleId, request.TerceroId, request.Monto, usuario);

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Distribución registrada exitosamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar distribución para detalle {DetalleId}.", request.DetalleId);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "No fue posible registrar la distribución."
                };
            }
        }

        // DELETE eliminar distribución
        public async Task<BusinessLogicResponse> EliminarDistribucionAsync(EliminarDistribucionRequest request, string? usuario)
        {
            try
            {
                if (request.Id <= 0 || request.DetalleId <= 0)
                    return new BusinessLogicResponse { StatusCode = 400, Message = "Distribución no válida." };

                var detalle = await _repository.ObtenerDetalleAsientoAsync(request.DetalleId);
                if (detalle is null)
                    return new BusinessLogicResponse { StatusCode = 404, Message = "No se encontró la línea de asiento." };

                var estadoActual = (detalle.EstadoCodigo ?? string.Empty).ToUpper().Trim();
                if (!_estadosPermitidos.Contains(estadoActual))
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 403,
                        Message = "No se puede modificar el prorrateo porque el asiento no está en estado permitido."
                    };
                }

                var dist = await _repository.ObtenerDistribucionPorIdAsync(request.Id, request.DetalleId);
                if (dist is null)
                    return new BusinessLogicResponse { StatusCode = 404, Message = "No se encontró la distribución a eliminar." };

                await _repository.EliminarDistribucionAsync(request.Id, request.DetalleId);

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Distribución eliminada exitosamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar distribución {Id}.", request.Id);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "No fue posible eliminar la distribución."
                };
            }
        }
    }

}