using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.Servicios
{

    public interface IRepositorioCuentas
    {
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int Id, int usuarioId);
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Eliminar(int id);
    }

    public class RepositorioCuentas : IRepositorioCuentas
    {

        private readonly string connectString;
        public RepositorioCuentas(IConfiguration conn)
        {
            connectString = conn.GetConnectionString("DefaultConnection");
        }


       
        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectString);

            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Cuentas(Nombre, TipoCuentaId, Descripcion, Balance)" +
                "VALUES(@Nombre, @TipoCuentaId,@Descripcion,@Balance);" +
                "SELECT SCOPE_IDENTITY();", cuenta );

            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectString);
            return await connection.QueryAsync<Cuenta>(@"SELECT Cuentas.Id, Cuentas.Nombre, Balance, tc.Nombre as TipoCuenta
                 FROM Cuentas
                 INNER JOIN TiposCuentas tc
                 ON tc.Id = Cuentas.TipoCuentaId
                 WHERE tc.UsuarioId = @UsuarioId
                 ORDER BY tc.Orden", new { usuarioId });
        }


        public async Task<Cuenta> ObtenerPorId(int Id, int usuarioId)
        {
            using var connection = new SqlConnection(connectString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(
                @"SELECT Cuentas.Id, Cuentas.Nombre, Balance,Cuentas.TipoCuentaId, Descripcion, TipoCuentaId
                 FROM Cuentas
                 INNER JOIN TiposCuentas tc
                 ON tc.Id = Cuentas.TipoCuentaId
                 WHERE tc.UsuarioId = @UsuarioId AND Cuentas.Id = @Id", new { Id, usuarioId });
        }


        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(connectString);

            var query = @"
                    UPDATE Cuentas
                    SET Nombre = @Nombre,
                     Balance = @Balance,
                     Descripcion = @Descripcion,
                     TipoCuentaId = @TipoCuentaId
                    WHERE Id = @Id;";

            await connection.ExecuteAsync(query, cuenta);
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(connectString);

            await connection.ExecuteAsync(@"DELETE Cuentas WHERE Id = @Id",
                new { id });

        }

    }
}
