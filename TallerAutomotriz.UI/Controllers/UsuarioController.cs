using Microsoft.AspNetCore.Mvc;

namespace TallerAutomotriz.UI.Controllers
{
    public class UsuarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Colaborador()
        {
            return View();
        }

        public IActionResult CrearUsuario()
        {
            return View();
        }
        public IActionResult MiPerfil()
        {
            return View("EditarUsuario"); 
        }

        public IActionResult EditarUsuario(int id)
        {
            return View();
        }


    }
}
