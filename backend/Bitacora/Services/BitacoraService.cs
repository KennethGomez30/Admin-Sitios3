using Bitacora.Entities;
using Bitacora.Repository;

namespace Bitacora.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly IBitacoraRepository _repository;
        private readonly ILogger<BitacoraService> _logger;

        public BitacoraService(IBitacoraRepository repository, ILogger<BitacoraService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<BusinessLogicResponse> RegistrarAsync(BitacoraRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Descripcion))
                return new BusinessLogicResponse
                {
                    StatusCode = 400,
                    Message = "El campo 'Descripcion' es requerido y no puede estar vacío."
                };

            var bitacora = new BitacoraEntity
            {
                FechaBitacora = DateTime.Now,
                Usuario = string.IsNullOrWhiteSpace(request.Usuario) ? null : request.Usuario.Trim(),
                Descripcion = request.Descripcion.Trim()
            };

            try
            {
                await _repository.Registrar(bitacora);

                return new BusinessLogicResponse
                {
                    StatusCode = 204,
                    Message = "Bitácora registrada exitosamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar bitácora para el usuario {Usuario}.", bitacora.Usuario);

                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Ocurrió un error interno al registrar la bitácora."
                };
            }
        }
    }
}