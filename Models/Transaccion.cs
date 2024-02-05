using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Display(Name = "Fecha de transacción")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;
        public decimal Monto { get; set; }
        [StringLength(maximumLength:1000, ErrorMessage = "La nota no puede pasar llos {1} caracteres")]
        public string Nota  { get; set; }
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]

        public int CuentaId { get; set; }

        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]
        public int CategoriaId { get; set; }
        [Display(Name = "Tipo de operación")]
        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingreso;
        public string Cuenta { get; set; }
        public string Categoria { get; set; }

    }
}
