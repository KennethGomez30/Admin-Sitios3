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

    // LISTAR
    public async Task<IEnumerable<ContactoEntity>> ListarPorTerceroAsync(int terceroId, string? usuario)
    {
        if (terceroId <= 0)
            return Enumerable.Empty<ContactoEntity>();

        var contactos = await _repository.ListarPorTerceroAsync(terceroId);

        _ = _bitacoraService.RegistrarAsync(
            $"El usuario consulta contactos del tercero {terceroId}",
            usuario
        );

        return contactos;
    }

    // OBTENER
    public async Task<ContactoEntity?> ObtenerPorIdAsync(int id, string? usuario)
    {
        var contacto = await _repository.ObtenerPorIdAsync(id);
        if (contacto is null)
            return null;

        _ = _bitacoraService.RegistrarAsync(
            $"El usuario consulta contacto {id}",
            usuario
        );

        return contacto;
    }

    // CREAR
    public async Task<(ContactoEntity? contacto, string? error)> CrearAsync(ContactoEntity c, string? usuario)
    {
        try
        {
            var errorValidacion = ValidarCamposBasicos(c);
            if (errorValidacion is not null)
                return (null, errorValidacion);

            if (!await _repository.ExisteTerceroAsync(c.TerceroId))
                return (null, "El tercero indicado no existe.");

            c.UsuarioCreacion = usuario;
            c.Id = await _repository.CrearAsync(c);

            _ = _bitacoraService.RegistrarAsync(
                $"Crear contacto: {JsonSerializer.Serialize(c)}",
                usuario
            );

            return (c, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear contacto para tercero {TerceroId}.", c.TerceroId);
            return (null, "Error interno al crear el contacto.");
        }
    }

    // ACTUALIZAR
    public async Task<(ContactoEntity? contacto, string? error, bool notFound)> ActualizarAsync(int id, ContactoEntity c, string? usuario)
    {
        try
        {
            var anterior = await _repository.ObtenerPorIdAsync(id);
            if (anterior is null)
                return (null, "El contacto no existe.", true);

            // No permitir cambiar de tercero al editar
            c.Id = id;
            c.TerceroId = anterior.TerceroId;

            var errorValidacion = ValidarCamposBasicos(c);
            if (errorValidacion is not null)
                return (null, errorValidacion, false);

            c.UsuarioModificacion = usuario;

            var ok = await _repository.ActualizarAsync(c);
            if (!ok)
                return (null, "No se pudo actualizar el contacto.", false);

            var actual = await _repository.ObtenerPorIdAsync(id);

            _ = _bitacoraService.RegistrarAsync(
                $"Actualizar contacto: {JsonSerializer.Serialize(new { anterior, actual })}",
                usuario
            );

            return (actual, null, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar contacto {Id}.", id);
            return (null, "Error interno al actualizar el contacto.", false);
        }
    }

    // ELIMINAR
    public async Task<(bool ok, string? error, bool notFound)> EliminarAsync(int id, string? usuario)
    {
        try
        {
            var actual = await _repository.ObtenerPorIdAsync(id);
            if (actual is null)
                return (false, "El contacto no existe.", true);

            var ok = await _repository.EliminarAsync(id);
            if (!ok)
                return (false, "No se pudo eliminar el contacto.", false);

            _ = _bitacoraService.RegistrarAsync(
                $"Eliminar contacto: {JsonSerializer.Serialize(actual)}",
                usuario
            );

            return (true, null, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar contacto {Id}.", id);
            return (false, "No se puede eliminar un registro con datos relacionados.", false);
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