using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Models
{
    public class CuentaCreacionViewModel: Cuenta
    {
        public IEnumerable<SelectListItem> Tipo { get; set; }
    }
}
