namespace AUX11.Entities
{
    public interface IDireccionService
    {
        Task<IEnumerable<DireccionEntity>> ListarPorTerceroAsync(int terceroId, string? usuario);
        Task<DireccionEntity?> ObtenerPorIdAsync(int id, string? usuario);
        Task<(DireccionEntity? direccion, string? error)> CrearAsync(DireccionEntity direccion, string? usuario);
        Task<(DireccionEntity? direccion, string? error, bool notFound)> ActualizarAsync(int id, DireccionEntity direccion, string? usuario);
        Task<(bool ok, string? error, bool notFound)> EliminarAsync(int id, string? usuario);
    }
}
