using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Validaciones;

namespace WebApplication1.Models
{
    public class TipoCuenta //: IValidatableObject
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="El campo {0} es requerido")]
        [PrimerLetraMayuscula]
        [Remote(action: "ExisteTipoCuenta",controller:"TiposCuentas")]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
           
        //    if(Nombre != null && Nombre.Length > 0)
        //    {
        //        var primeraLetra = Nombre[0].ToString();
        //        if(primeraLetra != primeraLetra.ToUpper())
        //        {
        //            yield return new ValidationResult("La primer letra debe ser may+uscula", new[] { nameof(Nombre) });
        //        }
        //    }

        //}

        /*ejemplo de otras validaciones 
        [Required(ErrorMessage ="El campo {0} es requerido")]
        [EmailAddress(ErrorMessage ="Debe ser un email válido")]
        public string Email { get; set; }
        [Range(minimum:18, maximum:150, ErrorMessage ="El rango es de 18 a 150")]
        public int Edad { get; set; }
        [Url(ErrorMessage ="Debe ser una url válida")]
        public string URL { get; set; }
        [CreditCard(ErrorMessage ="La tarjeta ingresada no es válida")]
        [Display(Name = "Tarjeta de crédito")]
        public string TarjetaCredito { get; set; }*/

    }
}
