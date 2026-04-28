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

        private static readonly string[] TiposValidos =
            { "Principal", "Facturación", "Envío", "Sucursal", "Otro" };

        public DireccionService(
            IDireccionRepository repository,
            IBitacoraService bitacoraService,
            ILogger<DireccionService> logger)
        {
            _repository = repository;
            _bitacoraService = bitacoraService;
            _logger = logger;
        }

        // LISTAR
        public async Task<IEnumerable<DireccionEntity>> ListarPorTerceroAsync(int terceroId, string? usuario)
        {
            if (terceroId <= 0)
                return Enumerable.Empty<DireccionEntity>();

            var direcciones = await _repository.ListarPorTerceroAsync(terceroId);

            _ = _bitacoraService.RegistrarAsync(
                $"El usuario consulta direcciones del tercero {terceroId}",
                usuario
            );

            return direcciones;
        }

        // OBTENER
        public async Task<DireccionEntity?> ObtenerPorIdAsync(int id, string? usuario)
        {
            var direccion = await _repository.ObtenerPorIdAsync(id);
            if (direccion is null)
                return null;

            _ = _bitacoraService.RegistrarAsync(
                $"El usuario consulta dirección {id}",
                usuario
            );

            return direccion;
        }

        // CREAR
        public async Task<(DireccionEntity? direccion, string? error)> CrearAsync(DireccionEntity d, string? usuario)
        {
            try
            {
                var errorValidacion = ValidarCamposBasicos(d);
                if (errorValidacion is not null)
                    return (null, errorValidacion);

                if (!await _repository.ExisteTerceroAsync(d.TerceroId))
                    return (null, "El tercero indicado no existe.");

                d.UsuarioCreacion = usuario;

                // Regla: solo una dirección puede ser principal por tercero.
                // Si la nueva viene como principal, desmarcar las anteriores.
                if (d.EsPrincipal)
                    await _repository.QuitarPrincipalDeTerceroAsync(d.TerceroId);   // ← ¡esta línea!

                d.Id = await _repository.CrearAsync(d);

                _ = _bitacoraService.RegistrarAsync(
                    $"Crear dirección: {JsonSerializer.Serialize(d)}",
                    usuario
                );

                return (d, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear dirección para tercero {TerceroId}.", d.TerceroId);
                return (null, "Error interno al crear la dirección.");
            }
        }

        // ACTUALIZAR
        public async Task<(DireccionEntity? direccion, string? error, bool notFound)> ActualizarAsync(int id, DireccionEntity d, string? usuario)
        {
            try
            {
                var anterior = await _repository.ObtenerPorIdAsync(id);
                if (anterior is null)
                    return (null, "La dirección no existe.", true);

                // No permitir cambiar de tercero al editar
                d.Id = id;
                d.TerceroId = anterior.TerceroId;

                var errorValidacion = ValidarCamposBasicos(d);
                if (errorValidacion is not null)
                    return (null, errorValidacion, false);

                d.UsuarioModificacion = usuario;

                // Regla del principal único:
                // si esta dirección queda como principal, desmarcar las OTRAS del tercero.
                if (d.EsPrincipal)
                    await _repository.QuitarPrincipalDeTerceroAsync(d.TerceroId, exceptoId: id);

                var ok = await _repository.ActualizarAsync(d);
                if (!ok)
                    return (null, "No se pudo actualizar la dirección.", false);

                var actual = await _repository.ObtenerPorIdAsync(id);

                _ = _bitacoraService.RegistrarAsync(
                    $"Actualizar dirección: {JsonSerializer.Serialize(new { anterior, actual })}",
                    usuario
                );

                return (actual, null, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar dirección {Id}.", id);
                return (null, "Error interno al actualizar la dirección.", false);
            }
        }

        // ELIMINAR
        public async Task<(bool ok, string? error, bool notFound)> EliminarAsync(int id, string? usuario)
        {
            try
            {
                var actual = await _repository.ObtenerPorIdAsync(id);
                if (actual is null)
                    return (false, "La dirección no existe.", true);

                var ok = await _repository.EliminarAsync(id);
                if (!ok)
                    return (false, "No se pudo eliminar la dirección.", false);

                _ = _bitacoraService.RegistrarAsync(
                    $"Eliminar dirección: {JsonSerializer.Serialize(actual)}",
                    usuario
                );

                return (true, null, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar dirección {Id}.", id);
                return (false, "No se puede eliminar un registro con datos relacionados.", false);
            }
        }

        // ---------- Helpers ----------

        private static string? ValidarCamposBasicos(DireccionEntity d)
        {
            if (d.TerceroId <= 0)
                return "Debe indicar un tercero válido.";

            if (string.IsNullOrWhiteSpace(d.DireccionExacta))
                return "La dirección exacta es requerida.";

            if (d.DireccionExacta.Length > 250)
                return "La dirección exacta no debe superar los 250 caracteres.";

            if (d.Estado != "Activa" && d.Estado != "Inactiva")
                return "El estado debe ser 'Activa' o 'Inactiva'.";

            return null;
        }
    }
}
