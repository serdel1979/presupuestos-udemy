using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Abstractions;
using System.Data;
using System.Reflection;
using WebApplication1.Models;
using WebApplication1.Servicios;

namespace WebApplication1.Controllers
{
    public class TransaccionesController: Controller
    {
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;
        private readonly IServicioReporte servicioReporte;

        public TransaccionesController(IServicioUsuarios servicioUsuarios, 
                                        IRepositorioCuentas repositorioCuentas,
                                        IRepositorioCategorias repositorioCategorias,
                                        IRepositorioTransacciones repositorioTransacciones,
                                        IMapper mapper,
                                        IServicioReporte servicioReporte)
        {
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
            this.servicioReporte = servicioReporte;
        }

        [Authorize]
        public async Task<IActionResult> Index(int mes, int año)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var modelo = await servicioReporte.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, año, ViewBag);

            return View(modelo);
        }

        public async Task<IActionResult> Semanal(int mes, int año)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();                

            IEnumerable<ObtenerTransaccionesPorSemana> porSemana = await servicioReporte.ObtenerSemanal(usuarioId, mes, año, ViewBag);

            var agrupado = porSemana.GroupBy(x => x.Semana)
                .Select(x => new ObtenerTransaccionesPorSemana()
                {
                    Semana = x.Key,
                    Ingresos = x.Where(x=>x.TipoOperacionId == TipoOperacion.Ingreso)
                        .Select(x=>x.Monto).FirstOrDefault(),
                    Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                        .Select(x => x.Monto).FirstOrDefault(),
                }).ToList();

            if(año == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                año = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(año, mes, 1);
            var diasDelMe = Enumerable.Range(1,fechaReferencia.AddMonths(1).AddDays(-1).Day);
            var diasSegmentados = diasDelMe.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if (grupoSemana is null)
                {
                    agrupado.Add(new ObtenerTransaccionesPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            
            }
            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel()
            {
                FechaReferencia = fechaReferencia,
                TransaccionesPorSemana = agrupado,
            };

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int año)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            if (año == 0)
            {
                año = DateTime.Today.Year;
            }

            var transacciones = await repositorioTransacciones.ObtenerTransaccionesPorMes(usuarioId, año);

            var transaccionesAgrupadas = transacciones.GroupBy(x => x.Mes)
                .Select(x => new ObtenerTransaccionesPorMes()
                {
                    Mes = x.Key,
                    Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                    Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault(),

                }).ToList();
            for (int mes = 1; mes <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(año, mes, 1);
                if (transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ObtenerTransaccionesPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia,
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel()
            {
                Año = año,
                TransaccionesPorMes = transaccionesAgrupadas,
            };

            return View(modelo);
        }

        public IActionResult ExcelReporte()
        {
            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int año)
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var usuarioId = servicioUsuarios.GetUsuarioId();

            var transacciones = await repositorioTransacciones
                .ObtenerTransaccionesPorUsuarioId(new ParamTransaccionesPorUsuario()
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });
            var nombreArchivo = $"Presupuestos - {fechaInicio.ToString("MMM yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);
        }


        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAño(int año)
        {
            var fechaInicio = new DateTime(año, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);

            var usuarioId = servicioUsuarios.GetUsuarioId();
            var transacciones = await repositorioTransacciones
               .ObtenerTransaccionesPorUsuarioId(new ParamTransaccionesPorUsuario()
               {
                   UsuarioId = usuarioId,
                   FechaInicio = fechaInicio,
                   FechaFin = fechaFin
               });
            var nombreArchivo = $"Presupuestos - {fechaInicio.ToString("yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);

        }


        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo()
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(1000);

            var usuarioId = servicioUsuarios.GetUsuarioId();
            var transacciones = await repositorioTransacciones
               .ObtenerTransaccionesPorUsuarioId(new ParamTransaccionesPorUsuario()
               {
                   UsuarioId = usuarioId,
                   FechaInicio = fechaInicio,
                   FechaFin = fechaFin
               });
            var nombreArchivo = $"Presupuestos - {DateTime.Today.ToString("dd-mm-YYYY")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);

        }
        private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                 new DataColumn("Fecha"),
                 new DataColumn("Cuenta"),
                 new DataColumn("Categoría"),
                 new DataColumn("Nota"),
                 new DataColumn("Monto"),
                 new DataColumn("Ingreso/Gasto"),
            });

            foreach (var transaccion in transacciones)
            {
                dataTable.Rows.Add(
                    transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId);
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
                }
            }
        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> 
            ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var transacciones = await repositorioTransacciones
              .ObtenerTransaccionesPorUsuarioId(new ParamTransaccionesPorUsuario()
              {
                  UsuarioId = usuarioId,
                  FechaInicio = start,
                  FechaFin = end
              });

            var eventosCalendario = transacciones
                .Select(transaccion => new EventoCalendario()
                {
                    Title = transaccion.Monto.ToString("N"),
                    Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                    End = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                    Color = (transaccion.TipoOperacionId == TipoOperacion.Gasto? "Red" : null)
                });
            return Json(eventosCalendario);
        }

       
        
        public async Task<JsonResult> ObtenerTransaccionesPorFecha
            (DateTime fecha)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var transacciones = await repositorioTransacciones
              .ObtenerTransaccionesPorUsuarioId(new ParamTransaccionesPorUsuario()
              {
                  UsuarioId = usuarioId,
                  FechaInicio = fecha,
                  FechaFin = fecha
              });

            return Json(transacciones);
        }
        
        
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
                
            modelo.Cuentas = await GetCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId,modelo.TipoOperacionId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<ActionResult> Crear(TransaccionCreacionViewModel modelo)
        {

            var usuarioId = servicioUsuarios.GetUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await GetCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }


            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioId = usuarioId;
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.Monto *= -1;
            }


            await repositorioTransacciones.Crear(modelo);
            
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> GetCuentas(int usuarioId)
        {
            var cuantas = await repositorioCuentas.Buscar(usuarioId);
            return cuantas.Select(x=> new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int UsuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.ObtenerCategorias(UsuarioId, tipoOperacion);
            return categorias.Select(x=>new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<ActionResult> getCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }


        [HttpGet]
        public async Task<IActionResult> Editar(int Id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(Id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }

            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);

            modelo.MontoAnterior = modelo.Monto;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
        
            modelo.Cuentas = await GetCuentas(usuarioId);

            modelo.UrlRetorno = urlRetorno;

            return View(modelo);
        }

        [HttpPost]
        public async Task<ActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await GetCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId,modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId,usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }

            var categoria = await repositorioCategorias.ObtenerCategorias(usuarioId,modelo.TipoOperacionId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = mapper.Map<Transaccion>(modelo);
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.Monto *= -1;
            }

            await repositorioTransacciones.Actualizar(transaccion,
                modelo.MontoAnterior, modelo.CuentaAnteriorId);

            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno);
            }
            
           
        }


        [HttpPost]
        public async Task<IActionResult> Borrar(int Id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var transaccion = await repositorioTransacciones.ObtenerPorId(Id,usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }

            await repositorioTransacciones.Borrar(Id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }

        }
    
    }
}
