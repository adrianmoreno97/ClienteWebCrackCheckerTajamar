using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClienteWebCrackChecker.Filters;
using ClienteWebCrackChecker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGetCrackChecker.Helpers;
using NuGetCrackChecker.Models;

namespace ClienteWebCrackChecker.Controllers
{
    
    public class UsuariosController : Controller
    {
        RepositoryProyecto repo;
        public UsuariosController(RepositoryProyecto repo)
        {
            this.repo = repo;
        }

        // GET: Perfil
        [UsuariosAuthorize]
        public async Task<IActionResult> Perfil()
        {
            String token = HttpContext.Session.GetString("Token");
            Usuario u = await this.repo.PerfilUsuarioAsync(token);
            if(u != null)
            {
                if (u.Activo == 0 && u.Role == "Usuario")
                {
                    return RedirectToAction("SinActivar", "Manage", new { id = u.IdUsuario, email = u.Email });
                }
                return View(u);
            }
            else
            {
                return RedirectToAction("CerrarSesion", "Manage");
            }
        }

        // GET: Editar
        [UsuariosAuthorize]
        public async Task<IActionResult> Editar(int id)
        {
            Usuario u = await repo.BuscarUsuarioAsync(id);
            return View(u);
        }

        // POST: Editar
        [UsuariosAuthorize]
        [HttpPost]
        public async Task<ActionResult> Editar(int id, String email, String role, String nombre, String apellidos, DateTime fechaNac, int telefono, String username, IFormFile archivo)
        {
            if (email.Length == 0 || fechaNac == null)
            {
                ViewData["Mensaje"] = "El campo email o la fecha de nacimiento están vacíos";
                return View(await repo.BuscarUsuarioAsync(id));
            }
            else
            {
                String token = HttpContext.Session.GetString("Token");
                if (archivo != null)
                {
                    String ruta = await repo.SubirImagenAsync(archivo, token);
                    await repo.EditarUsuarioAsync(id, email, role, nombre, apellidos, fechaNac, telefono, username, ruta, token);
                }
                else
                {
                    await repo.EditarUsuarioAsync(id, email, role, nombre, apellidos, fechaNac, telefono, username, null, token);
                }
                return RedirectToAction("CerrarSesion","Manage");
            }
        }

        // GET: DarBaja
        [UsuariosAuthorize]
        public async Task<IActionResult> DarBaja(int id)
        {
            String token = HttpContext.Session.GetString("Token");
            await this.repo.DarBajaCuentaAsync(id, token);
            return RedirectToAction("CerrarSesion", "Manage");
        }

        // GET: ListaDeseados
        [UsuariosAuthorize]
        public async Task<IActionResult> ListaUsuario(int id)
        {
            Usuario u = await this.repo.BuscarUsuarioAsync(id);
            if(u.Activo == 0)
            {
                return RedirectToAction("SinActivar", "Manage");
            }
            else
            {
                String token = HttpContext.Session.GetString("Token");
                List<string> lista = await this.repo.GetIdJuegoListaAsync(id, token);
                List<Juego> listadeseados = new List<Juego>();
                if (lista[0].Length != 0 && lista != null)
                {
                    foreach (String l in lista)
                    {
                        listadeseados.Add(await repo.BuscarJuegoIdAsync(l));
                    }
                }
                return View(listadeseados);
            }
        }

        // GET: AnyadirJuego
        [UsuariosAuthorize]
        public async Task<IActionResult> AnyadirJuego(String idjuego, int iduser)
        {
            String token = HttpContext.Session.GetString("Token");
            await this.repo.AnyadirJuegoListaDeseadosAsync(idjuego, iduser, token);
            return RedirectToAction("ListaUsuario", "Usuarios", new { id = iduser });
        }

        // GET: EliminarJuego
        [UsuariosAuthorize]
        public async Task<IActionResult> EliminarJuego(String idjuego, int iduser)
        {
            String token = HttpContext.Session.GetString("Token");
            await this.repo.EliminarJuegoListaDeseadosAsync(idjuego, iduser, token);
            return RedirectToAction("ListaUsuario", "Usuarios", new { id = iduser });
        }

        // GET: CambiarPass
        [UsuariosAuthorize]
        public async Task<IActionResult> CambiarPass(int id)
        {
            Usuario u = await repo.BuscarUsuarioAsync(id);
            return View(u);
        }

        // POST: CambiarPass
        [UsuariosAuthorize]
        [HttpPost]
        public async Task<IActionResult> CambiarPass(int id, String passanterior, String passnuevo, String confpassnuevo)
        {
            Usuario u = await repo.BuscarUsuarioAsync(id);
            String salt = u.Salt;
            byte[] txtACifrar = CypherHelper.CifradoHashSHA256(passanterior + salt);
            if (CypherHelper.CompararBytes(txtACifrar, u.Password) == false)
            {
                ViewData["Error"] = "La contraseña anterior no es correcta";
                return View(u);
            }

            else if (passnuevo.Length < 6)
            {
                ViewData["Error"] = "La longitud de la contraseña nueva debe de tener al menos 6 caractéres";
                return View(u);
            }
            else if (passnuevo != confpassnuevo)
            {
                ViewData["Error"] = "Las nuevas contraseñas no coinciden";
                return View(u);
            }
            else if (passanterior == passnuevo)
            {
                ViewData["Error"] = "La contraseña anterior es la misma que la nueva";
                return View(u);
            }
            else
            {
                String token = HttpContext.Session.GetString("Token");
                await repo.CambiarPasswordAsync(id, passnuevo, token);
                return RedirectToAction("CerrarSesion", "Manage");
            }
        }

        public IActionResult RecuperarPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarPassword(String email)
        {
            Usuario u = await this.repo.BuscarUsuarioAsync(email);
            if(u != null)
            {
                String msj = await this.repo.EnviarEmailRecuperacionAsync(email);
                ViewData["MensajeExito"] = "El email de recuperación ha sido enviado a tu correo. Por favor revíselo.";
                return View();
            }
            else
            {
                ViewData["Mensaje"] = "No hay registrado ningún usuario con ese email.";
                return View();
            }
        }
    }
}