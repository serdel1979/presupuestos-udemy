﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UsuariosController: Controller
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;

        public UsuariosController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }


        public IActionResult Registro()
        {
            return View();  
        }

        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {


            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new Usuario { Email = modelo.Email };
            var resultado = await userManager.CreateAsync(usuario, password: modelo.Password);

            if (resultado.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index","Transacciones");
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(modelo);
            }

        }



        [HttpPost]
        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            return RedirectToAction("Login", "Usuarios");
        }


        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(ModelViewLogin loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            var resultado = await signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password,loginViewModel.Recordar,
                lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                ModelState.AddModelError(string.Empty,"Usuario o contraseña incorrectos");
                return View(loginViewModel);
            }
        }


    }
}
