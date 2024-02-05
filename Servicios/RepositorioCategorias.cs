using Dapper;
using Microsoft.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Servicios
{
    public interface IRepositorioCategorias
    {
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Listar(int usuarioId);

        Task<Categoria> ObtenerPorId(int id, int usuarioId);
        Task Eliminar(int id);
        Task Editar(Categoria categoria);

        Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion);
    }
    public class RepositorioCategorias: IRepositorioCategorias
    {
        private readonly string connectionString;

        public RepositorioCategorias(
            IConfiguration configuration
            )
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task<IEnumerable<Categoria>> Listar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@"SELECT * FROM Categorias WHERE UsuarioId = @usuarioId;",
                new { usuarioId });

        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Categorias(Nombre, TipoOperacionId, UsuarioId)" +
                "VALUES(@Nombre, @TipoOperacionId,@UsuarioId);" +
                "SELECT SCOPE_IDENTITY();", categoria);

            categoria.Id = id;
        }


        public async Task Editar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"UPDATE Categorias
                            SET Nombre = @Nombre,
                                TipoOperacionId = @TipoOperacionId,
                                UsuarioId = @UsuarioId
                            WHERE Id = @Id;";

            await connection.ExecuteAsync(query, categoria);
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Categoria>(@"SELECT * FROM Categorias WHERE Id = @Id and UsuarioId = @UsuarioId",
                new { id, usuarioId });

        }

        //ObtenerCategorias(int UsuarioId, TipoOperacion tipoOperacion)

        public async Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@"SELECT * FROM Categorias WHERE UsuarioId = @usuarioId AND TipoOperacionId = @tipoOperacion;",
                new { usuarioId, tipoOperacion });

        }


        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(@"DELETE Categorias WHERE Id = @Id",
                new { id });

        }


    }
}
