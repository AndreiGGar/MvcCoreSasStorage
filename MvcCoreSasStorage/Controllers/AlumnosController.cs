using Microsoft.AspNetCore.Mvc;
using MvcCoreSasStorage.Models;
using MvcCoreSasStorage.Services;

namespace MvcCoreSasStorage.Controllers
{
    public class AlumnosController : Controller
    {
        private ServiceStorageTables service;

        public AlumnosController(ServiceStorageTables service)
        {
            this.service = service;
        }

        public IActionResult Token()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Token(string curso)
        {
            string data = await this.service.GetTokenAsync(curso);
            ViewData["TOKEN"] = data;
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string token)
        {
            List<Alumno> alumnos = await this.service.GetAlumnosAsync(token);
            return View(alumnos);
        }
    }
}
