using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ClienteWebCrackChecker.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGetCrackChecker.Helpers;
using NuGetCrackChecker.Models;

namespace ClienteWebCrackChecker.Controllers
{
    public class ManageController : Controller
    {
        RepositoryProyecto repo;
        public ManageController(RepositoryProyecto repo)
        {
            this.repo = repo;
        }

        public async Task ClearUserCookies()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (HttpContext.Session.GetString("Token") != null)
            {
                HttpContext.Session.Remove("Token");
            }
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(String email, String password)
        {
            String token = await this.repo.GetToken(email, password);
            if (token == null)
            {
                ViewData["Mensaje"] = "Usuario/Password incorrectos";
                return View();
            }
            else
            {
                // Recuperar al usuario que se ha validado
                Usuario u = await this.repo.PerfilUsuarioAsync(token);
                // Habilitamos la seguridad de mvc core con claims
                ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                // Almacenamos el numero de empleado
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, u.IdUsuario.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Name, u.Nombre));
                identity.AddClaim(new Claim(ClaimTypes.Role, u.Role));
                identity.AddClaim(new Claim("Apellidos", u.Apellidos));
                identity.AddClaim(new Claim("Username", u.Username));
                identity.AddClaim(new Claim("Foto", u.Foto));
                identity.AddClaim(new Claim("Id", u.IdUsuario.ToString()));
                identity.AddClaim(new Claim("Email", u.Email));
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.Now.AddMinutes(60)
                });
                // Debemos almacenar el token una vez que el usuario ya existe
                HttpContext.Session.SetString("Token", token);
                return RedirectToAction("Perfil", "Usuarios");
            }
        }

        public async Task<IActionResult> CerrarSesion()
        {
            /*await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (HttpContext.Session.GetString("Token") != null)
            {
                HttpContext.Session.Remove("Token");
            }*/
            await this.ClearUserCookies();
            return RedirectToAction("Index", "Juegos");
        }

        // GET: Registro
        public IActionResult Registro()
        {
            return View();
        }

        // POST: Registro
        [HttpPost]
        public async Task<IActionResult> Registro(String email, String password, String username, String confirmarEmail, String confirmarPassword)
        {
            ViewData["Email"] = email;
            ViewData["Username"] = username;
            String mensaje = await repo.ComprobarDisponibilidadAsync(email, username);
            if (mensaje.Length != 0)
            {
                ViewData["Mensaje"] = mensaje;
                return View();
            }
            else if (password.Length == 0 || (password != confirmarPassword))
            {
                ViewData["Mensaje"] = "La contraseña está vacía o las contraseñas no coinciden";
                return View();
            }
            else if (email != confirmarEmail)
            {
                ViewData["Mensaje"] = "Los emails introducidos no coinciden";
                return View();
            }
            else
            {
                String salt = CypherHelper.GenerarSalt();
                String txtACifrar = password + salt;
                byte[] cifrado = CypherHelper.CifradoHashSHA256(txtACifrar);
                int id = await repo.RegistrarUsuarioAsync(email, cifrado, salt, username);
                return RedirectToAction("EnviarEmailActivacion", new { id = id, email = email });
            }
        }

        public async Task<IActionResult> EnviarEmailActivacion(int id, String email)
        {
            String msj = await this.repo.EnviarEmailActivacionAsync(email, id);
            ViewData["Mensaje"] = msj;
            return View();
        }

        public async Task<IActionResult> ActivarCuenta(int id)
        {
            await repo.ActivarCuentaAsync(id);
            await this.ClearUserCookies();
            return View();
        }

        public IActionResult SinActivar()
        {
            return View();
        }

        public IActionResult SinPermisos()
        {
            return View();
        }

        public async Task<IActionResult> RestablecerPass(String email)
        {
            Usuario u = await repo.BuscarUsuarioAsync(email);
            return View(u);
        }

        // POST: CambiarPass
        [HttpPost]
        public async Task<IActionResult> RestablecerPass(int id, String pass, String confpass)
        {
            Usuario u = await repo.BuscarUsuarioAsync(id);

            if (pass.Length < 6)
            {
                ViewData["Error"] = "La longitud de la contraseña nueva debe de tener al menos 6 caractéres";
                return View(u);
            }
            else if (pass != confpass)
            {
                ViewData["Error"] = "Las nuevas contraseñas no coinciden";
                return View(u);
            }
            else
            {
                String token = HttpContext.Session.GetString("Token");
                await repo.CambiarPasswordAsync(id, pass, token);
                return RedirectToAction("CerrarSesion", "Manage");
            }
        }
    }
}