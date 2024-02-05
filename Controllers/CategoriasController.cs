using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Servicios;

namespace WebApplication1.Controllers
{
    public class CategoriasController: Controller
    {
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IServicioUsuarios servicioUsuarios;

        public CategoriasController(IRepositorioCategorias repositorioCategorias, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioCategorias = repositorioCategorias;
            this.servicioUsuarios = servicioUsuarios;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var usuarioId = servicioUsuarios.GetUsuarioId();
            var categorias = await repositorioCategorias.Listar(usuarioId);

            return View(categorias);
        }

            [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }
            var usuarioId = servicioUsuarios.GetUsuarioId();
            categoria.UsuarioId = usuarioId;

            await repositorioCategorias.Crear(categoria);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Eliminar(int Id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var categoria = await repositorioCategorias.ObtenerPorId(Id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }


        [HttpPost]
        public async Task<IActionResult> EliminarCategoria(int Id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var categoria = await repositorioCategorias.ObtenerPorId(Id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCategorias.Eliminar(Id);

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Editar(int Id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var categoria = await repositorioCategorias.ObtenerPorId(Id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var catExiste = await repositorioCategorias.ObtenerPorId(categoria.Id, usuarioId);

            if (catExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            categoria.UsuarioId = usuarioId;
            await repositorioCategorias.Editar(categoria);

            return RedirectToAction("Index");
        }



    }
}
