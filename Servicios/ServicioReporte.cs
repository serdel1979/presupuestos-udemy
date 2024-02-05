using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models;

namespace WebApplication1.Servicios
{

    public interface IServicioReporte
    {
        Task<ReporteTransaccionesDetallada> ObtenerReporteTransaccionesDetalladaPorCuenta
            (int usuarioId, int cuentaId, int mes, int año, dynamic Viewbag);

        Task<ReporteTransaccionesDetallada> ObtenerReporteTransaccionesDetalladas
            (int usuarioId, int mes, int año, dynamic ViewBag);
        Task<IEnumerable<ObtenerTransaccionesPorSemana>>
            ObtenerSemanal(int usuarioId, int mes, int año, dynamic ViewBag);
    }

    public class ServicioReporte : IServicioReporte
    {

        private readonly HttpContext httpContext;

        public IRepositorioTransacciones RepositorioTransacciones { get; }

        public ServicioReporte(IRepositorioTransacciones repositorioTransacciones, IHttpContextAccessor
            httpContextAccessor)
        {
            this.httpContext = httpContextAccessor.HttpContext;
            RepositorioTransacciones = repositorioTransacciones;
        }

        public async Task<ReporteTransaccionesDetallada> ObtenerReporteTransaccionesDetalladas
            (int usuarioId, int mes, int año,dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GeneraFechaInicioYFin(mes, año);

            var parametro = new ParamTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await RepositorioTransacciones.ObtenerTransaccionesPorUsuarioId(parametro);
            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
            SetViewBag(ViewBag, fechaInicio);

            return modelo;
        }


        public async Task<ReporteTransaccionesDetallada> ObtenerReporteTransaccionesDetalladaPorCuenta
            (int usuarioId, int cuentaId, int mes, int año, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GeneraFechaInicioYFin(mes, año);

            var modeloTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = cuentaId,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await RepositorioTransacciones
               .ObtenerTransaccionesPorCuenta(modeloTransaccionesPorCuenta);

            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

            SetViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        private void SetViewBag(dynamic ViewBag, DateTime fechaInicio)
        {
            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.añoAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.añoPosterior = fechaInicio.AddMonths(1).Year;
            ViewBag.urlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;
        }

        private static ReporteTransaccionesDetallada GenerarReporteTransaccionesDetalladas(DateTime fechaInicio, DateTime fechaFin, IEnumerable<Transaccion> transacciones)
        {
            var modelo = new ReporteTransaccionesDetallada();

            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                                            .GroupBy(x => x.FechaTransaccion)
                                            .Select(grupo => new ReporteTransaccionesDetallada.TransaccionesPorFecha()
                                            {
                                                FechaTransaccion = grupo.Key,
                                                Transacciones = grupo.AsEnumerable(),
                                            });

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;
            return modelo;
        }

        private (DateTime fechaInicio, DateTime fechaFin)
            GeneraFechaInicioYFin(int mes, int año)
        {
            DateTime fechaInicio;
            DateTime fechaFin;

            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio, fechaFin);
        }

    
        public async Task<IEnumerable<ObtenerTransaccionesPorSemana>>
            ObtenerSemanal(int usuarioId, int mes, int año, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GeneraFechaInicioYFin(mes,año);

            var parametro = new ParamTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
            };

            SetViewBag(ViewBag, fechaInicio);
                    
            return await RepositorioTransacciones.ObtenerTransaccionesPorSemana(parametro);

        }
    
    
    }
}
