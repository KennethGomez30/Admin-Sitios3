using System.Text.Json;
using System.Text.RegularExpressions;
using AUX12.Entities;
using AUX12.Repository;

namespace AUX12.Services;

public class ContactoService : IContactoService
{
    private readonly IContactoRepository _repository;
    private readonly IBitacoraService _bitacoraService;
    private readonly ILogger<ContactoService> _logger;

    private static readonly string[] TiposValidos =
        { "Principal", "Facturación", "Cobros", "Soporte", "Otro" };

    public ContactoService(
        IContactoRepository repository,
        IBitacoraService bitacoraService,
        ILogger<ContactoService> logger)
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

            var contactos = await _repository.ListarPorTerceroAsync(terceroId);

            _ = _bitacoraService.RegistrarAsync(
                $"El usuario consulta contactos del tercero {terceroId}",
                usuario
            );

            return new BusinessLogicResponse
            {
                StatusCode = 200,
                Message = "OK",
                ResponseObject = contactos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar contactos del tercero {TerceroId}.", terceroId);
            return new BusinessLogicResponse
            {
                StatusCode = 500,
                Message = "Error interno al listar contactos."
            };
        }
    }

    // OBTENER por id
    public async Task<BusinessLogicResponse> ObtenerPorIdAsync(int id, string? usuario)
    {
        try
        {
            var contacto = await _repository.ObtenerPorIdAsync(id);
            if (contacto is null)
                return new BusinessLogicResponse
                {
                    StatusCode = 404,
                    Message = "El contacto no existe."
                };

            _ = _bitacoraService.RegistrarAsync(
                $"El usuario consulta contacto {id}",
                usuario
            );

            return new BusinessLogicResponse
            {
                StatusCode = 200,
                Message = "OK",
                ResponseObject = contacto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contacto {Id}.", id);
            return new BusinessLogicResponse
            {
                StatusCode = 500,
                Message = "Error interno al obtener el contacto."
            };
        }
    }

    // CREAR
    public async Task<BusinessLogicResponse> CrearAsync(ContactoEntity c, string? usuario)
    {
        try
        {
            var errorValidacion = ValidarCamposBasicos(c);
            if (errorValidacion is not null)
                return new BusinessLogicResponse { StatusCode = 400, Message = errorValidacion };

            if (!await _repository.ExisteTerceroAsync(c.TerceroId))
                return new BusinessLogicResponse
                {
                    StatusCode = 400,
                    Message = "El tercero indicado no existe."
                };

            c.UsuarioCreacion = usuario;
            c.Id = await _repository.CrearAsync(c);

            _ = _bitacoraService.RegistrarAsync(
                $"Crear contacto: {JsonSerializer.Serialize(c)}",
                usuario
            );

            return new BusinessLogicResponse
            {
                StatusCode = 201,
                Message = "Contacto creado correctamente.",
                ResponseObject = c
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear contacto para tercero {TerceroId}.", c.TerceroId);
            return new BusinessLogicResponse
            {
                StatusCode = 500,
                Message = "Error interno al crear el contacto."
            };
        }
    }

    // ACTUALIZAR
    public async Task<BusinessLogicResponse> ActualizarAsync(int id, ContactoEntity c, string? usuario)
    {
        try
        {
            var anterior = await _repository.ObtenerPorIdAsync(id);
            if (anterior is null)
                return new BusinessLogicResponse
                {
                    StatusCode = 404,
                    Message = "El contacto no existe."
                };

            // No permitir cambiar de tercero al editar
            c.Id = id;
            c.TerceroId = anterior.TerceroId;

            var errorValidacion = ValidarCamposBasicos(c);
            if (errorValidacion is not null)
                return new BusinessLogicResponse { StatusCode = 400, Message = errorValidacion };

            c.UsuarioModificacion = usuario;

            var ok = await _repository.ActualizarAsync(c);
            if (!ok)
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "No se pudo actualizar el contacto."
                };

            var actual = await _repository.ObtenerPorIdAsync(id);

            _ = _bitacoraService.RegistrarAsync(
                $"Actualizar contacto: {JsonSerializer.Serialize(new { anterior, actual })}",
                usuario
            );

            return new BusinessLogicResponse
            {
                StatusCode = 200,
                Message = "Contacto actualizado correctamente.",
                ResponseObject = actual
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar contacto {Id}.", id);
            return new BusinessLogicResponse
            {
                StatusCode = 500,
                Message = "Error interno al actualizar el contacto."
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
                    Message = "El contacto no existe."
                };

            var ok = await _repository.EliminarAsync(id);
            if (!ok)
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "No se pudo eliminar el contacto."
                };

            _ = _bitacoraService.RegistrarAsync(
                $"Eliminar contacto: {JsonSerializer.Serialize(actual)}",
                usuario
            );

            return new BusinessLogicResponse
            {
                StatusCode = 200,
                Message = "Contacto eliminado correctamente."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar contacto {Id}.", id);
            return new BusinessLogicResponse
            {
                StatusCode = 409,
                Message = "No se puede eliminar un registro con datos relacionados."
            };
        }
    }

    // ---------- Helpers ----------

    private static string? ValidarCamposBasicos(ContactoEntity c)
    {
        if (c.TerceroId <= 0)
            return "Debe indicar un tercero válido.";

        if (string.IsNullOrWhiteSpace(c.Nombre))
            return "El nombre del contacto es requerido.";

        if (c.Nombre.Length > 100)
            return "El nombre no debe superar los 100 caracteres.";

        if (!string.IsNullOrEmpty(c.Cargo) && c.Cargo.Length > 100)
            return "El cargo no debe superar los 100 caracteres.";

        if (!string.IsNullOrWhiteSpace(c.Email))
        {
            if (c.Email.Length > 100)
                return "El email no debe superar los 100 caracteres.";

            // Validación simple de formato
            if (!Regex.IsMatch(c.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return "El email no tiene un formato válido.";
        }

        if (!string.IsNullOrWhiteSpace(c.Telefono) && c.Telefono.Length > 20)
            return "El teléfono no debe superar los 20 caracteres.";

        if (string.IsNullOrWhiteSpace(c.Tipo) || !TiposValidos.Contains(c.Tipo))
            return "El tipo debe ser Principal, Facturación, Cobros, Soporte u Otro.";

        if (c.Estado != "Activo" && c.Estado != "Inactivo")
            return "El estado debe ser 'Activo' o 'Inactivo'.";

        return null;
    }
}