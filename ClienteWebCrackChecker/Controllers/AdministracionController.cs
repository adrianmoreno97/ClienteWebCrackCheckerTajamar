using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClienteWebCrackChecker.Filters;
using ClienteWebCrackChecker.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGetCrackChecker.Models;

namespace ClienteWebCrackChecker.Controllers
{
    [AdminAuthorize]
    public class AdministracionController : Controller
    {
        RepositoryProyecto repo;
        public AdministracionController(RepositoryProyecto repo)
        {
            this.repo = repo;
        }
        
        public async Task<IActionResult> ListadoUsuarios()
        {
            String token = HttpContext.Session.GetString("Token");
            return View(await this.repo.GetUsuariosAsync(token));
        }

        // GET: EditarUsuario
        public async Task<IActionResult> EditarUsuario(int id)
        {
            Usuario u = await repo.BuscarUsuarioAsync(id);
            return View(u);
        }

        // POST: EditarUsuario
        [HttpPost]
        public async Task<IActionResult> EditarUsuario(int id, String role, int activo)
        {
            String token = HttpContext.Session.GetString("Token");
            await repo.EditarUsuarioAdminAsync(id, role, activo, token);
            return RedirectToAction("ListadoUsuarios");
        }

        // GET: EliminarUsuario
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            String token = HttpContext.Session.GetString("Token");
            await repo.EliminarUsuarioAsync(id, token);
            return RedirectToAction("ListadoUsuarios");
        }
    }
}