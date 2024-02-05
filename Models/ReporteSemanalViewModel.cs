namespace WebApplication1.Models
{
    public class ReporteSemanalViewModel
    {
        public decimal Ingresos => TransaccionesPorSemana.Sum(x=> x.Ingresos);
        public decimal Gastos => TransaccionesPorSemana.Sum(_ => _.Gastos);
        public decimal Total => Ingresos - Gastos;
        public DateTime FechaReferencia { get; set; }
        public IEnumerable<ObtenerTransaccionesPorSemana> TransaccionesPorSemana { get; set; }
    }
}
