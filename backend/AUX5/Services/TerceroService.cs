using AUX5;
using AUX5.Entities;
using AUX5.Repository;
using System.Text.Json;

namespace AUX5.Services
{
    public class TerceroService : ITerceroService
    {
        private readonly ITerceroRepository _repository;
        private readonly IBitacoraService _bitacora;
        private readonly ILogger<TerceroService> _logger;

        public TerceroService(
            ITerceroRepository repository,
            IBitacoraService bitacora,
            ILogger<TerceroService> logger)
        {
            _repository = repository;
            _bitacora = bitacora;
            _logger = logger;
        }

        // GET /api/Terceros
        public async Task<BusinessLogicResponse> ListarAsync(string usuario)
        {
            try
            {
                var data = await _repository.ListarAsync();

                _ = _bitacora.RegistrarAsync("El usuario consulta Terceros", usuario);

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Terceros obtenidos exitosamente.",
                    ResponseObject = data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar terceros.");
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Ocurrió un error interno al listar los terceros."
                };
            }
        }

        // GET /api/Terceros/{id}
        public async Task<BusinessLogicResponse> ObtenerPorIdAsync(int id, string usuario)
        {
            try
            {
                var tercero = await _repository.ObtenerPorIdAsync(id);

                if (tercero is null)
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "Tercero no encontrado."
                    };

                _ = _bitacora.RegistrarAsync(
                    $"El usuario consulta Tercero con id {id}",
                    usuario
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Tercero obtenido exitosamente.",
                    ResponseObject = tercero
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tercero {Id}.", id);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Ocurrió un error interno al obtener el tercero."
                };
            }
        }

        // POST /api/Terceros
        public async Task<BusinessLogicResponse> CrearAsync(CrearTerceroRequest request, string usuario)
        {
            try
            {
                // Catálogos dinámicos desde BD
                var (tipos, estados) = await ObtenerCatalogosAsync();

                var error = ValidarCrear(request, tipos, estados);
                if (error is not null)
                    return new BusinessLogicResponse { StatusCode = 400, Message = error };

                if (await _repository.ExisteIdentificacionAsync(request.Identificacion.Trim()))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 409,
                        Message = "Ya existe un tercero con esa identificación."
                    };

                var entidad = new TerceroEntity
                {
                    Identificacion = request.Identificacion.Trim(),
                    NombreRazonSocial = request.NombreRazonSocial.Trim(),
                    Tipo = request.Tipo.Trim(),
                    Email = NullIfEmpty(request.Email),
                    Telefono = NullIfEmpty(request.Telefono),
                    Estado = request.Estado.Trim()
                };

                // Repositorio inserta y devuelve el registro completo desde la BD
                var creado = await _repository.CrearAsync(entidad, usuario);

                _ = _bitacora.RegistrarAsync(
                    JsonSerializer.Serialize(new
                    {
                        Motivo = "El usuario crea Tercero",
                        Identificacion = creado.Identificacion,
                        NombreRazonSocial = creado.NombreRazonSocial,
                        Tipo = creado.Tipo,
                        Email = creado.Email,
                        Telefono = creado.Telefono,
                        Estado = creado.Estado
                    }, new JsonSerializerOptions { WriteIndented = false }),
                    usuario
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 201,
                    Message = "Tercero creado exitosamente.",
                    ResponseObject = creado
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear tercero {Identificacion}.", request.Identificacion);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Ocurrió un error interno al crear el tercero."
                };
            }
        }

        // PUT /api/Terceros/{id}
        public async Task<BusinessLogicResponse> ActualizarAsync(
            int id, ActualizarTerceroRequest request, string usuario)
        {
            try
            {
                var anterior = await _repository.ObtenerPorIdAsync(id);
                if (anterior is null)
                    return new BusinessLogicResponse { StatusCode = 404, Message = "Tercero no encontrado." };

                var (tipos, estados) = await ObtenerCatalogosAsync();

                var error = ValidarActualizar(request, tipos, estados);
                if (error is not null)
                    return new BusinessLogicResponse { StatusCode = 400, Message = error };

                var entidad = new TerceroEntity
                {
                    Id = id,
                    Identificacion = anterior.Identificacion, // inmutable
                    NombreRazonSocial = request.NombreRazonSocial.Trim(),
                    Tipo = request.Tipo.Trim(),
                    Email = NullIfEmpty(request.Email),
                    Telefono = NullIfEmpty(request.Telefono),
                    Estado = request.Estado.Trim()
                };

                // Repositorio actualiza y devuelve el registro completo desde la BD
                var actualizado = await _repository.ActualizarAsync(entidad, usuario);

                _ = _bitacora.RegistrarAsync(
                    JsonSerializer.Serialize(new
                    {
                        Motivo = "El usuario actualiza Tercero",
                        Anterior = new
                        {
                            anterior.NombreRazonSocial,
                            anterior.Tipo,
                            anterior.Email,
                            anterior.Telefono,
                            anterior.Estado
                        },
                        Nuevo = new
                        {
                            actualizado.NombreRazonSocial,
                            actualizado.Tipo,
                            actualizado.Email,
                            actualizado.Telefono,
                            actualizado.Estado
                        }
                    }, new JsonSerializerOptions { WriteIndented = false }),
                    usuario
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Tercero actualizado exitosamente.",
                    ResponseObject = actualizado
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar tercero {Id}.", id);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Ocurrió un error interno al actualizar el tercero."
                };
            }
        }

        // DELETE /api/Terceros/{id}
        public async Task<BusinessLogicResponse> EliminarAsync(int id, string usuario)
        {
            try
            {
                var tercero = await _repository.ObtenerPorIdAsync(id);
                if (tercero is null)
                    return new BusinessLogicResponse { StatusCode = 404, Message = "Tercero no encontrado." };

                if (await _repository.TieneRelacionesAsync(id))
                {
                    _ = _bitacora.RegistrarAsync(
                        $"Intento fallido de eliminar Tercero '{tercero.Identificacion}' - Tiene datos relacionados",
                        usuario
                    );

                    return new BusinessLogicResponse
                    {
                        StatusCode = 409,
                        Message = "No se puede eliminar un tercero con datos relacionados."
                    };
                }

                await _repository.EliminarAsync(id);

                _ = _bitacora.RegistrarAsync(
                    JsonSerializer.Serialize(new
                    {
                        Motivo = "El usuario elimina Tercero",
                        Identificacion = tercero.Identificacion,
                        NombreRazonSocial = tercero.NombreRazonSocial,
                        Tipo = tercero.Tipo,
                        Email = tercero.Email,
                        Telefono = tercero.Telefono,
                        Estado = tercero.Estado
                    }, new JsonSerializerOptions { WriteIndented = false }),
                    usuario
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 204,
                    Message = "Tercero eliminado exitosamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar tercero {Id}.", id);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Ocurrió un error interno al eliminar el tercero."
                };
            }
        }

        // Helpers

        private async Task<(IEnumerable<string> tipos, IEnumerable<string> estados)> ObtenerCatalogosAsync()
        {
            var tiposTask = _repository.ObtenerTiposAsync();
            var estadosTask = _repository.ObtenerEstadosAsync();
            await Task.WhenAll(tiposTask, estadosTask);
            return (tiposTask.Result, estadosTask.Result);
        }

        private static string? ValidarCrear(
            CrearTerceroRequest r,
            IEnumerable<string> tipos,
            IEnumerable<string> estados)
        {
            if (string.IsNullOrWhiteSpace(r.Identificacion))
                return "El campo 'Identificacion' es requerido.";
            if (r.Identificacion.Length > 50)
                return "La identificación no puede superar 50 caracteres.";
            if (string.IsNullOrWhiteSpace(r.NombreRazonSocial))
                return "El campo 'NombreRazonSocial' es requerido.";
            if (r.NombreRazonSocial.Length > 200)
                return "El nombre / razón social no puede superar 200 caracteres.";
            if (!tipos.Contains(r.Tipo))
                return $"El tipo no es válido. Valores permitidos: {string.Join(", ", tipos)}.";
            if (!string.IsNullOrWhiteSpace(r.Email) && !IsValidEmail(r.Email))
                return "El formato del correo electrónico no es válido.";
            if (!string.IsNullOrWhiteSpace(r.Telefono) && !IsValidPhone(r.Telefono))
                return "El formato del teléfono no es válido.";
            if (!estados.Contains(r.Estado))
                return $"El estado no es válido. Valores permitidos: {string.Join(", ", estados)}.";

            return null;
        }

        private static string? ValidarActualizar(
            ActualizarTerceroRequest r,
            IEnumerable<string> tipos,
            IEnumerable<string> estados)
        {
            if (string.IsNullOrWhiteSpace(r.NombreRazonSocial))
                return "El campo 'NombreRazonSocial' es requerido.";
            if (r.NombreRazonSocial.Length > 200)
                return "El nombre / razón social no puede superar 200 caracteres.";
            if (!tipos.Contains(r.Tipo))
                return $"El tipo no es válido. Valores permitidos: {string.Join(", ", tipos)}.";
            if (!string.IsNullOrWhiteSpace(r.Email) && !IsValidEmail(r.Email))
                return "El formato del correo electrónico no es válido.";
            if (!string.IsNullOrWhiteSpace(r.Telefono) && !IsValidPhone(r.Telefono))
                return "El formato del teléfono no es válido.";
            if (!estados.Contains(r.Estado))
                return $"El estado no es válido. Valores permitidos: {string.Join(", ", estados)}.";

            return null;
        }

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static bool IsValidEmail(string email) =>
            System.Net.Mail.MailAddress.TryCreate(email, out _);

        private static bool IsValidPhone(string phone) =>
            System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[\d\s\+\-\(\)]{7,20}$");
    }
}