using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Abstractions;
using System.Reflection;
using WebApplication1.Models;
using WebApplication1.Servicios;

namespace WebApplication1.Controllers
{
    public class CuentasController: Controller
    {
        private IServicioUsuarios servicioUsuarios;
        private IRepositorioTiposCuentas repositorioTipoCuentas;
        private readonly IMapper mapper;
        private IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioReporte servicioReporte;

        public CuentasController(IRepositorioTiposCuentas repoTipoCuentas, 
            IMapper mapper, IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas,
            IRepositorioTransacciones repositorioTransacciones,
            IServicioReporte servicioReporte)
        {
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioTipoCuentas = repoTipoCuentas;
            this.mapper = mapper;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioReporte = servicioReporte;
        }



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var cuentasConTipoCuenta = await repositorioCuentas.Buscar(usuarioId);
            var modelo = cuentasConTipoCuenta
                .GroupBy(x => x.TipoCuenta)
                .Select(grupo => new IndiceCuentasViewModel
                {
                    TipoCuenta = grupo.Key,
                    Cuentas = grupo.AsEnumerable()
                }).ToList();

            return View(modelo);
        }


        public async Task<IActionResult> Detalle(int Id, int mes, int año)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(Id, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }

            var modelo = await servicioReporte.
                ObtenerReporteTransaccionesDetalladaPorCuenta(usuarioId, cuenta.Id, mes, año, ViewBag);

            ViewBag.Cuenta = cuenta.Nombre;

            return View(modelo);

        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.Tipo = await ObtenerTiposCuentas(usuarioId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {

            var usuarioId = servicioUsuarios.GetUsuarioId();

            var tipoCuenta = await repositorioTipoCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);
            
            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.Tipo = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTipoCuentas.Listar(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }


        public async Task<IActionResult> Editar(int Id)
        {

            var usuarioId = servicioUsuarios.GetUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(Id,usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }


            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);

            modelo.Tipo = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }


        [HttpPost]
        public async Task<ActionResult> Editar(CuentaCreacionViewModel cuenta)
        {

            var usuarioId = servicioUsuarios.GetUsuarioId();
            var excuenta = await repositorioCuentas.ObtenerPorId(cuenta.Id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await repositorioCuentas
                .ObtenerPorId(excuenta.TipoCuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Actualizar(cuenta);

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Eliminar(int Id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var cuenta = await repositorioCuentas.ObtenerPorId(Id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCuenta(int Id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var cuenta = await repositorioCuentas.ObtenerPorId(Id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Eliminar(Id);

            return RedirectToAction("Index");
        }


    }
}
