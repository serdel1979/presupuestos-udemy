using Dapper;
using Microsoft.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Servicios
{

    public interface IRepositorioUsuarios
    {
        Task<int> CrearUsuario(Usuario usuario);
        Task<Usuario> BuscarUsuarioPorEmail(string EmailNormalizado);

    }
    public class RepositorioUsuarios : IRepositorioUsuarios
    {

        private readonly string connectionString;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }



        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection
                (connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO 
                        Usuarios (Email, EmailNormalizado, PasswordHash) 
                        VALUES (@Email,@EmailNormalizado,@PasswordHash);
                        SELECT SCOPE_IDENTITY();",
                usuario);

            return id;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string EmailNormalizado)
        {
            using var connection = new SqlConnection
                (connectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>(@"SELECT * 
                            FROM Usuarios WHERE EmailNormalizado = @EmailNormalizado",
                new { EmailNormalizado });
        }

    }
}
