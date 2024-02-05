using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ModelViewLogin
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo {0} debe ser un Email válido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Password { get; set; }

        public bool Recordar { get; set; }
    }
}
