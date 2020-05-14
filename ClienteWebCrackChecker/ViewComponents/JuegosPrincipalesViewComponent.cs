using ClienteWebCrackChecker.Repositories;
using Microsoft.AspNetCore.Mvc;
using NuGetCrackChecker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClienteWebCrackChecker.ViewComponents
{
    public class JuegosPrincipalesViewComponent: ViewComponent
    {
        RepositoryProyecto repo;
        public JuegosPrincipalesViewComponent(RepositoryProyecto repo)
        {
            this.repo = repo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Juego> juegos = new List<Juego>();
            juegos.Add(await repo.BuscarJuegoSlugAsync("cyberpunk-2077"));
            juegos.Add(await repo.BuscarJuegoSlugAsync("red-dead-redemption-2"));
            juegos.Add(await repo.BuscarJuegoSlugAsync("fifa-20"));
            juegos.Add(await repo.BuscarJuegoSlugAsync("dragon-ball-z-kakarot"));
            return View(juegos);
        }
    }
}
