using System.ComponentModel.DataAnnotations;
using WebApplication1.Validaciones;

namespace WebApplication1.Models
{
    public class Cuenta
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="El campo {0} es requerido")]
        [StringLength(maximumLength:50)]
        [PrimerLetraMayuscula]
        public string Nombre { get; set; }
        [Display(Name ="Tipo Cuenta")]
        public  int TipoCuentaId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public decimal Balance { get; set; }
        [StringLength(maximumLength:1000)]
        public string Descripcion { get; set; }
        public string TipoCuenta { get; set; }
    }
}
