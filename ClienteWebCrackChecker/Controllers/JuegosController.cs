using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClienteWebCrackChecker.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGetCrackChecker.Models;

namespace ClienteWebCrackChecker.Controllers
{
    public class JuegosController : Controller
    {
        RepositoryProyecto repo;
        public JuegosController(RepositoryProyecto repo)
        {
            this.repo = repo;
        }

        // GET: Juegos
        public async Task<IActionResult> Index(int? posicion, String busqueda)
        {
            if (posicion == null)
            {
                posicion = 0;
            }
            ViewData["Posicion"] = posicion;
            if (busqueda == null)
            {
                ViewData["Total"] = 573;
                return View(await repo.PaginarJuegosAsync(posicion.GetValueOrDefault()));
            }
            else if (busqueda.Length < 3)
            {
                ViewData["Error"] = "La búsqueda \'" + busqueda + "\' debe de ser de 3 caractéres o más";
                ViewData["Total"] = 573;
                return View(await repo.PaginarJuegosAsync(posicion.GetValueOrDefault()));
            }
            else
            {
                ViewData["Busqueda"] = busqueda;
                List<Juego> juegos = await repo.BuscarJuegosAsync(busqueda);
                ViewData["Total"] = juegos.Count() / 30;
                return View(juegos);
            }
        }

        // GET: _JuegosPrincipales
        public async Task<IActionResult> _JuegosPrincipales()
        {
            List<Juego> juegos = new List<Juego>();
            juegos.Add(await repo.BuscarJuegoSlugAsync("cyberpunk-2077"));
            juegos.Add(await repo.BuscarJuegoSlugAsync("red-dead-redemption-2"));
            juegos.Add(await repo.BuscarJuegoSlugAsync("fifa-20"));
            juegos.Add(await repo.BuscarJuegoSlugAsync("dragon-ball-z-kakarot"));
            return PartialView(juegos);
        }
        
        // GET: Detalles
        public async Task<IActionResult> Detalles(String id, String busqueda, int? posicion)
        {
            if (busqueda != null)
            {
                ViewData["Busqueda"] = busqueda;
            }
            else if (posicion == null)
            {
                posicion = 0;
            }
            ViewData["Posicion"] = posicion.GetValueOrDefault();
            Juego j = await repo.BuscarJuegoIdAsync(id);
            return View(j);
        }
        
        // GET: EditarJuego
        public async Task<IActionResult> EditarJuego(String id)
        {
            Juego j = await repo.BuscarJuegoIdAsync(id);
            return View(j);
        }

        // POST: EditarJuego
        [HttpPost]
        public async Task<IActionResult> EditarJuego(String id, int? cracked, DateTime fechaLanz)
        {
            String token = HttpContext.Session.GetString("Token");
            if (cracked == null)
            {
                cracked = 0;
            }
            if (cracked.GetValueOrDefault() == 1)
            {
                DateTime fechaCrack = DateTime.Now;
                await repo.EditarJuegoAsync(id, fechaCrack, fechaLanz, token);
            }
            else
            {
                DateTime fechaCrack = DateTime.Parse("1900-01-01");
                await repo.EditarJuegoAsync(id, fechaCrack, fechaLanz, token);
            }
            return RedirectToAction("Detalles", new { id = id });
        }
        
    }
}