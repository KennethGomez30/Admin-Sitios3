using AUX12.Entities;
using Dapper;

namespace AUX12.Repository;

public class ContactoRepository : IContactoRepository
{
    private readonly IDbConnectionFactory _factory;

    public ContactoRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<ContactoEntity>> ListarPorTerceroAsync(int terceroId)
    {
        using var con = _factory.CreateConnection();
        var sql = @"SELECT id, tercero_id, nombre, cargo, email, telefono,
                           tipo, estado,
                           fecha_creacion, usuario_creacion,
                           fecha_modificacion, usuario_modificacion
                    FROM tercero_contactos
                    WHERE tercero_id = @terceroId
                    ORDER BY
                      CASE tipo
                        WHEN 'Principal'   THEN 1
                        WHEN 'Facturación' THEN 2
                        WHEN 'Cobros'      THEN 3
                        WHEN 'Soporte'     THEN 4
                        ELSE 5
                      END,
                      id DESC";
        return await con.QueryAsync<ContactoEntity>(sql, new { terceroId });
    }

    public async Task<ContactoEntity?> ObtenerPorIdAsync(int id)
    {
        using var con = _factory.CreateConnection();
        var sql = @"SELECT id, tercero_id, nombre, cargo, email, telefono,
                           tipo, estado,
                           fecha_creacion, usuario_creacion,
                           fecha_modificacion, usuario_modificacion
                    FROM tercero_contactos
                    WHERE id = @id";
        return await con.QueryFirstOrDefaultAsync<ContactoEntity>(sql, new { id });
    }

    public async Task<int> CrearAsync(ContactoEntity c)
    {
        using var con = _factory.CreateConnection();
        var sql = @"INSERT INTO tercero_contactos
                    (tercero_id, nombre, cargo, email, telefono,
                     tipo, estado, usuario_creacion)
                    VALUES
                    (@TerceroId, @Nombre, @Cargo, @Email, @Telefono,
                     @Tipo, @Estado, @UsuarioCreacion);
                    SELECT LAST_INSERT_ID();";
        return await con.ExecuteScalarAsync<int>(sql, c);
    }

    public async Task<bool> ActualizarAsync(ContactoEntity c)
    {
        using var con = _factory.CreateConnection();
        var sql = @"UPDATE tercero_contactos
                    SET nombre = @Nombre,
                        cargo = @Cargo,
                        email = @Email,
                        telefono = @Telefono,
                        tipo = @Tipo,
                        estado = @Estado,
                        usuario_modificacion = @UsuarioModificacion
                    WHERE id = @Id";
        var rows = await con.ExecuteAsync(sql, c);
        return rows > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var con = _factory.CreateConnection();
        var sql = "DELETE FROM tercero_contactos WHERE id = @id";
        var rows = await con.ExecuteAsync(sql, new { id });
        return rows > 0;
    }

    public async Task<bool> ExisteTerceroAsync(int terceroId)
    {
        using var con = _factory.CreateConnection();
        var sql = "SELECT COUNT(1) FROM terceros WHERE id = @terceroId";
        var count = await con.ExecuteScalarAsync<int>(sql, new { terceroId });
        return count > 0;
    }
}