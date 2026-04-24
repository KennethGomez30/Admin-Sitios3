using AUX8Correcta.Entities;
using AUX8Correcta.Repository;
using System.Text.Json;

namespace AUX8Correcta.Services
{
    
        public class Tercerosservice : ITercerosservice
        {
            private readonly Itercerosrepository _repository;
            private readonly ILogger<Tercerosservice> _logger;

            // Estados que permiten prorratear: EA3 = Borrador, EA4 = Pendiente de aprobación
            private static readonly string[] _estadosPermitidos = ["EA3", "EA4"];

            public Tercerosservice(
                Itercerosrepository repository,
                ILogger<Tercerosservice> logger)
            {
                _repository = repository;
                _logger = logger;
            }

            /*

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
                    return new BusinessLogicResponse { StatusCode = 500, Message = "Error interno al obtener períodos." };
                }
            }

            // GET lineas por periodo (index.php)
            public async Task<BusinessLogicResponse> ObtenerLineasAsync(int periodoId)
            {
                try
                {
                    var lineas = await _repository.ObtenerLineasAsientoPorPeriodoAsync(periodoId);

                    _ = _bitacoraService.RegistrarAsync(
                        "El usuario consulta distribución por centro de costo"
                    );

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
                    return new BusinessLogicResponse { StatusCode = 500, Message = "Error interno al obtener líneas de asiento." };
                }
            }

            // GET detalle + distribuciones (prorrateo.php GET)
            public async Task<BusinessLogicResponse> ObtenerProrrateoAsync(int detalleId)
            {
                try
                {
                    var detalle = await _repository.ObtenerDetalleAsientoAsync(detalleId);

                    if (detalle is null)
                        return new BusinessLogicResponse
                        {
                            StatusCode = 404,
                            Message = "No se encontró la línea de asiento seleccionada."
                        };

                    var estadoActual = (detalle.EstadoCodigo ?? string.Empty).ToUpper().Trim();
                    if (!_estadosPermitidos.Contains(estadoActual))
                        return new BusinessLogicResponse
                        {
                            StatusCode = 403,
                            Message = "Solo se permite prorratear líneas de asientos en estado Borrador o Pendiente de aprobación."
                        };

                    var distribuciones = await _repository.ObtenerDistribucionesAsync(detalleId);
                    var centros = await _repository.ObtenerCentrosActivosAsync();
                    var montoDistribuido = distribuciones.Sum(d => d.Monto);

                    return new BusinessLogicResponse
                    {
                        StatusCode = 200,
                        Message = "OK",
                        ResponseObject = new
                        {
                            Detalle = detalle,
                            Distribuciones = distribuciones,
                            CentrosActivos = centros,
                            MontoDistribuido = montoDistribuido,
                            Pendiente = detalle.Monto - montoDistribuido
                        }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener prorrateo del detalle {DetalleId}.", detalleId);
                    return new BusinessLogicResponse { StatusCode = 500, Message = "Error interno al obtener información de prorrateo." };
                }
            }

            // GET centros activos
            public async Task<BusinessLogicResponse> ObtenerCentrosActivosAsync()
            {
                try
                {
                    var centros = await _repository.ObtenerCentrosActivosAsync();
                    return new BusinessLogicResponse { StatusCode = 200, Message = "OK", ResponseObject = centros };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener centros de costo activos.");
                    return new BusinessLogicResponse { StatusCode = 500, Message = "Error interno al obtener centros de costo." };
                }
            }

            // POST agregar distribución (prorrateo.php accion=agregar)
            public async Task<BusinessLogicResponse> AgregarDistribucionAsync(
                AgregarDistribucionRequest request, string? usuario)
            {
                try
                {
                    if (request.DetalleId <= 0)
                        return new BusinessLogicResponse { StatusCode = 400, Message = "Detalle de asiento no válido." };

                    if (request.CentroCostoId <= 0)
                        return new BusinessLogicResponse { StatusCode = 400, Message = "Debe seleccionar un centro de costo." };

                    if (request.Monto <= 0)
                        return new BusinessLogicResponse { StatusCode = 400, Message = "El monto debe ser mayor a cero." };

                    var detalle = await _repository.ObtenerDetalleAsientoAsync(request.DetalleId);
                    if (detalle is null)
                        return new BusinessLogicResponse { StatusCode = 404, Message = "No se encontró la línea de asiento seleccionada." };

                    var estadoActual = (detalle.EstadoCodigo ?? string.Empty).ToUpper().Trim();
                    if (!_estadosPermitidos.Contains(estadoActual))
                        return new BusinessLogicResponse
                        {
                            StatusCode = 403,
                            Message = "No se puede modificar el prorrateo porque el asiento no está en estado Borrador ni Pendiente de aprobación."
                        };

                    var centroValido = await _repository.CentroCostoExisteYActivoAsync(request.CentroCostoId);
                    if (!centroValido)
                        return new BusinessLogicResponse { StatusCode = 400, Message = "El centro de costo seleccionado no existe o no está activo." };

                    var distribuciones = await _repository.ObtenerDistribucionesAsync(request.DetalleId);
                    var montoDistribuido = distribuciones.Sum(d => d.Monto);

                    if (montoDistribuido + request.Monto - detalle.Monto > 0.009m)
                        return new BusinessLogicResponse { StatusCode = 400, Message = "La distribución excede el monto total de la línea." };

                    var yaExiste = await _repository.ExisteDistribucionAsync(request.DetalleId, request.CentroCostoId);
                    if (yaExiste)
                        return new BusinessLogicResponse { StatusCode = 400, Message = "Ese centro de costo ya fue agregado a esta línea." };

                    await _repository.AgregarDistribucionAsync(
                        request.DetalleId, request.CentroCostoId, request.Monto, usuario
                    );

                    _ = _bitacoraService.RegistrarAsync(
                        "El usuario registra distribución de centro de costo en detalle de asiento: " +
                        JsonSerializer.Serialize(new
                        {
                            detalle_id = request.DetalleId,
                            centrocosto_id = request.CentroCostoId,
                            monto = request.Monto
                        }, new JsonSerializerOptions { WriteIndented = false }),
                        usuario
                    );

                    return new BusinessLogicResponse { StatusCode = 200, Message = "Distribución registrada exitosamente." };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al agregar distribución para detalle {DetalleId}.", request.DetalleId);
                    return new BusinessLogicResponse { StatusCode = 500, Message = "No fue posible registrar la distribución." };
                }
            }

            // POST eliminar distribución (prorrateo.php accion=eliminar)
            public async Task<BusinessLogicResponse> EliminarDistribucionAsync(
                EliminarDistribucionRequest request, string? usuario)
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
                        return new BusinessLogicResponse
                        {
                            StatusCode = 403,
                            Message = "No se puede modificar el prorrateo porque el asiento no está en estado Borrador ni Pendiente de aprobación."
                        };

                    var dist = await _repository.ObtenerDistribucionPorIdAsync(request.Id, request.DetalleId);
                    if (dist is null)
                        return new BusinessLogicResponse { StatusCode = 404, Message = "No se encontró la distribución a eliminar." };

                    await _repository.EliminarDistribucionAsync(request.Id, request.DetalleId);

                    _ = _bitacoraService.RegistrarAsync(
                        "El usuario elimina distribución de centro de costo: " +
                        JsonSerializer.Serialize(new
                        {
                            id = dist.Id,
                            detalle_id = dist.DetalleId,
                            centrocosto_id = dist.CentroCostoId,
                            monto = dist.Monto
                        }, new JsonSerializerOptions { WriteIndented = false }),
                        usuario
                    );

                    return new BusinessLogicResponse { StatusCode = 200, Message = "Distribución eliminada exitosamente." };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al eliminar distribución {Id}.", request.Id);
                    return new BusinessLogicResponse { StatusCode = 500, Message = "No fue posible eliminar la distribución." };
                }
            }
                */
            
        }
}
