using System.Text.Json;
using AUX11.Entities;
using AUX11.Repository;

namespace AUX11.Services
{
    public class DireccionService : IDireccionService
    {
        private readonly IDireccionRepository _repository;
        private readonly IBitacoraService _bitacoraService;
        private readonly ILogger<DireccionService> _logger;

        public DireccionService(
            IDireccionRepository repository,
            IBitacoraService bitacoraService,
            ILogger<DireccionService> logger)
        {
            _repository = repository;
            _bitacoraService = bitacoraService;
            _logger = logger;
        }

        // LISTAR por tercero
        public async Task<BusinessLogicResponse> ListarPorTerceroAsync(int terceroId, string? usuario)
        {
            try
            {
                if (terceroId <= 0)
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Debe indicar un tercero válido."
                    };

                var direcciones = await _repository.ListarPorTerceroAsync(terceroId);

                _ = _bitacoraService.RegistrarAsync(
                    $"El usuario consulta direcciones del tercero {terceroId}",
                    usuario
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "OK",
                    ResponseObject = direcciones
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar direcciones del tercero {TerceroId}.", terceroId);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Error interno al listar direcciones."
                };
            }
        }

        // OBTENER por id
        public async Task<BusinessLogicResponse> ObtenerPorIdAsync(int id, string? usuario)
        {
            try
            {
                var direccion = await _repository.ObtenerPorIdAsync(id);
                if (direccion is null)
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "La dirección no existe."
                    };

                _ = _bitacoraService.RegistrarAsync(
                    $"El usuario consulta dirección {id}",
                    usuario
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "OK",
                    ResponseObject = direccion
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dirección {Id}.", id);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Error interno al obtener la dirección."
                };
            }
        }

        // CREAR
        public async Task<BusinessLogicResponse> CrearAsync(DireccionEntity d, string? usuario)
        {
            try
            {
                // Validaciones
                var errorValidacion = ValidarCamposBasicos(d);
                if (errorValidacion is not null)
                    return new BusinessLogicResponse { StatusCode = 400, Message = errorValidacion };

                if (!await _repository.ExisteTerceroAsync(d.TerceroId))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El tercero indicado no existe."
                    };

                d.UsuarioCreacion = usuario;

                // Regla: solo una dirección puede ser principal
                if (d.EsPrincipal)
                    await _repository.QuitarPrincipalDeTerceroAsync(d.TerceroId);

                d.Id = await _repository.CrearAsync(d);

                _ = _bitacoraService.RegistrarAsync(
                    $"Crear dirección: {JsonSerializer.Serialize(d)}",
                    usuario
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 201,
                    Message = "Dirección creada correctamente.",
                    ResponseObject = d
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear dirección para tercero {TerceroId}.", d.TerceroId);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Error interno al crear la dirección."
                };
            }
        }

        // ACTUALIZAR
        public async Task<BusinessLogicResponse> ActualizarAsync(int id, DireccionEntity d, string? usuario)
        {
            try
            {
                var anterior = await _repository.ObtenerPorIdAsync(id);
                if (anterior is null)
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "La dirección no existe."
                    };

                // No permitir cambiar de tercero al editar
                d.Id = id;
                d.TerceroId = anterior.TerceroId;

                var errorValidacion = ValidarCamposBasicos(d);
                if (errorValidacion is not null)
                    return new BusinessLogicResponse { StatusCode = 400, Message = errorValidacion };

                d.UsuarioModificacion = usuario;

                // Regla del principal único (excluyendo la que estamos editando)
                if (d.EsPrincipal)
                    await _repository.QuitarPrincipalDeTerceroAsync(d.TerceroId, exceptoId: id);

                var ok = await _repository.ActualizarAsync(d);
                if (!ok)
                    return new BusinessLogicResponse
                    {
                        StatusCode = 500,
                        Message = "No se pudo actualizar la dirección."
                    };

                var actual = await _repository.ObtenerPorIdAsync(id);

                _ = _bitacoraService.RegistrarAsync(
                    $"Actualizar dirección: {JsonSerializer.Serialize(new { anterior, actual })}",
                    usuario
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Dirección actualizada correctamente.",
                    ResponseObject = actual
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar dirección {Id}.", id);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Error interno al actualizar la dirección."
                };
            }
        }

        // ELIMINAR
        public async Task<BusinessLogicResponse> EliminarAsync(int id, string? usuario)
        {
            try
            {
                var actual = await _repository.ObtenerPorIdAsync(id);
                if (actual is null)
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "La dirección no existe."
                    };

                var ok = await _repository.EliminarAsync(id);
                if (!ok)
                    return new BusinessLogicResponse
                    {
                        StatusCode = 500,
                        Message = "No se pudo eliminar la dirección."
                    };

                _ = _bitacoraService.RegistrarAsync(
                    $"Eliminar dirección: {JsonSerializer.Serialize(actual)}",
                    usuario
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Dirección eliminada correctamente."
                };
            }
            catch (Exception ex)
            {
                // Si en el futuro otra tabla referencia a tercero_direcciones,
                // la FK va a tronar acá y devolvemos el mensaje del enunciado.
                _logger.LogError(ex, "Error al eliminar dirección {Id}.", id);
                return new BusinessLogicResponse
                {
                    StatusCode = 409,
                    Message = "No se puede eliminar un registro con datos relacionados."
                };
            }
        }

        // ---------- Helpers ----------

        private static string? ValidarCamposBasicos(DireccionEntity d)
        {
            if (d.TerceroId <= 0)
                return "Debe indicar un tercero válido.";

            if (string.IsNullOrWhiteSpace(d.Alias))
                return "El alias es requerido.";

            if (d.Alias.Length > 100)
                return "El alias no debe superar los 100 caracteres.";

            if (string.IsNullOrWhiteSpace(d.DireccionExacta))
                return "La dirección exacta es requerida.";

            if (d.Estado != "Activa" && d.Estado != "Inactiva")
                return "El estado debe ser 'Activa' o 'Inactiva'.";

            return null;
        }
    }
}
