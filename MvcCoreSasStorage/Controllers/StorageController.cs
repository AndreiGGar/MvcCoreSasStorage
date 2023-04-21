using Microsoft.AspNetCore.Mvc;
using MvcCoreSasStorage.Models;
using MvcCoreSasStorage.Services;

namespace MvcCoreSasStorage.Controllers
{
    public class StorageController : Controller
    {
        private ServiceStorageTables service;

        public StorageController(ServiceStorageTables service)
        {
            this.service = service;
        }

        public async Task<IActionResult> Index()
        {
            List<Alumno> alumnos = await service.GetAlumnosAsync();
            return View(alumnos);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string curso)
        {
            List<Alumno> alumnos;
            if (string.IsNullOrEmpty(curso))
            {
                alumnos = await service.GetAlumnosAsync();
            }
            else
            {
                alumnos = await service.GetAlumnosCursoAsync(curso);
            }

            ViewData["CURSO"] = curso;
            return View(alumnos);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Alumno alumno)
        {
            if (ModelState.IsValid)
            {
                await service.CreateAlumnoAsync(alumno.IdAlumno, alumno.Nombre, alumno.Apellidos, alumno.Nota, alumno.Curso);
                return RedirectToAction("Index");
            }
            return View(alumno);
        }
    }
}
