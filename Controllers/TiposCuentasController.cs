using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using WebApplication1.Models;
using WebApplication1.Servicios;

namespace WebApplication1.Controllers
{
    public class TiposCuentasController: Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTipoCuentas;
        private IServicioUsuarios servicioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTipoCuentas, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioTipoCuentas = repositorioTipoCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }


        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var tiposCuentas = await repositorioTipoCuentas.Listar(usuarioId);
            return View(tiposCuentas);
        }

        public IActionResult Crear()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = servicioUsuarios.GetUsuarioId();
            var existe = await repositorioTipoCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
          
            if (existe)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe");
                return View(tipoCuenta);
            }
            await repositorioTipoCuentas.Crear(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ExisteTipoCuenta(string nombre)
        {
            var id = servicioUsuarios.GetUsuarioId();
            var existe = await repositorioTipoCuentas.Existe(nombre, id);

            if (existe)
            {
                return Json($"{nombre} ya existe");
            }

            return Json(true);
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var tipoCuenta = await repositorioTipoCuentas.ObtenerPorId(id,usuarioId);

            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }


        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var tipoExiste = await repositorioTipoCuentas.Existe(tipoCuenta.Nombre, usuarioId);   
            
            if (tipoExiste)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTipoCuentas.Actualizar(tipoCuenta);

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Eliminar(int Id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var tipoCuenta = await repositorioTipoCuentas.ObtenerPorId(Id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarTipoCuenta(int Id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var tipoCuenta = await repositorioTipoCuentas.ObtenerPorId(Id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTipoCuentas.Eliminar(Id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var tiposCuentas = await repositorioTipoCuentas.Listar(usuarioId);

            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoPertenecen = ids.Except(idsTiposCuentas).ToList();

            if(idsTiposCuentasNoPertenecen.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenadas = ids.Select((val,indic)=>
            new TipoCuenta() {Id = val, Orden = indic + 1 }).AsEnumerable();

            await repositorioTipoCuentas.Ordenar(tiposCuentasOrdenadas);

            return Ok();
        }
    }
}
