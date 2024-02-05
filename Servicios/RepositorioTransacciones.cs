using Dapper;
using Microsoft.Data.SqlClient;
using System.Threading;
using WebApplication1.Models;

namespace WebApplication1.Servicios
{

    public interface IRepositorioTransacciones
    {
        Task Crear(Transaccion transaccion);
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);

        Task Borrar(int Id);
        Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorCuenta(ObtenerTransaccionesPorCuenta modelo);
        Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorUsuarioId(ParamTransaccionesPorUsuario modelo);

        Task<IEnumerable<ObtenerTransaccionesPorSemana>> ObtenerTransaccionesPorSemana(ParamTransaccionesPorUsuario modelo);

        Task<IEnumerable<ObtenerTransaccionesPorMes>>ObtenerTransaccionesPorMes(int usuarioId, int año);

    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;

        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("Transacciones_insertar", new
            {
                transaccion.UsuarioId,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.CategoriaId,
                transaccion.CuentaId,
                transaccion.Nota
            },
            commandType: System.Data.CommandType.StoredProcedure);
            transaccion.Id = id;
        }

        //Transacciones_Actualizar
        public async Task Actualizar(Transaccion transaccion,
            decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.ExecuteAsync("Transacciones_Actualizar", new
            {
                transaccion.Id,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.CategoriaId,
                transaccion.CuentaId,
                transaccion.Nota,
                montoAnterior,
                cuentaAnteriorId
            },
            commandType: System.Data.CommandType.StoredProcedure);
        }


        public async Task Borrar(int Id)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.ExecuteAsync("Transacciones_Borrar", new
            {
               Id
            },
            commandType: System.Data.CommandType.StoredProcedure);
        }


        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"SELECT Transacciones.*, cat.TipoOperacionId
                                                                    FROM Transacciones
                                                                    INNER JOIN Categorias cat
                                                                    ON cat.Id = Transacciones.CategoriaId
                                                                    WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId", new { id, usuarioId });
        }


        public async Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorCuenta(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                (@"select t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, 
                            cu.Nombre as Cuenta, c.TipoOperacionId
                                            from Transacciones t
                                            inner join Categorias c
                                            on c.Id = t.CategoriaId
                                            inner join Cuentas cu
                                            on cu.Id = t.CuentaId
                                            where t.CuentaId = @CuentaId and t.UsuarioId = @UsuarioId
                                            and FechaTransaccion between @FechaInicio and @FechaFin",modelo);

        }



        public async Task<IEnumerable<ObtenerTransaccionesPorSemana>> ObtenerTransaccionesPorSemana(ParamTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ObtenerTransaccionesPorSemana>
                (@"select datediff(d, @fechaInicio, FechaTransaccion) / 7 + 1 as Semana,
                                sum(Monto) as Monto, cat.TipoOperacionId
                                from Transacciones
                                inner join Categorias cat
                                on cat.Id = Transacciones.CategoriaId
                                where Transacciones.UsuarioId = @UsuarioId and
                                FechaTransaccion between @fechaInicio and @fechaFin
                                group by datediff(d, @fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionId", modelo);

        }

        public async Task<IEnumerable<ObtenerTransaccionesPorMes>> 
            ObtenerTransaccionesPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ObtenerTransaccionesPorMes>
                (@"select MONTH(FechaTransaccion) as Mes,
                    SUM(Monto) as Monto, cat.TipoOperacionId
                    from Transacciones
                    inner join Categorias cat
                    on cat.Id = Transacciones.CategoriaId
                    where Transacciones.UsuarioId = @usuarioId and year(FechaTransaccion) = @Año
                    group by Month(FechaTransaccion), cat.TipoOperacionId", new { usuarioId, año});

        }



        public async Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorUsuarioId(ParamTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                (@"select t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, 
                            cu.Nombre as Cuenta, c.TipoOperacionId, Nota
                                            from Transacciones t
                                            inner join Categorias c
                                            on c.Id = t.CategoriaId
                                            inner join Cuentas cu
                                            on cu.Id = t.CuentaId
                                            where t.UsuarioId = @UsuarioId
                                            and FechaTransaccion between @FechaInicio and @FechaFin
                                            ORDER BY t.FechaTransaccion DESC", modelo);

        }



    }
}
