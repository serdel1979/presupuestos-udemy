using System.Security.Claims;

namespace WebApplication1.Servicios
{

    public interface IServicioUsuarios
    {
        int GetUsuarioId();
    }
    public class ServicioUsuarios : IServicioUsuarios
    {

        private readonly HttpContext httpContext;
        public ServicioUsuarios(IHttpContextAccessor httpContextAccesor)
        {
            httpContext = httpContextAccesor.HttpContext;
        }


        public int GetUsuarioId()
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User.Claims
                    .Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

                var id = int.Parse(idClaim.Value);
                return id;
            }
            else
            {
                return httpContext.Response.StatusCode;
            }

        }
    }
}
