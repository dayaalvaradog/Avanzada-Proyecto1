using Microsoft.AspNetCore.Mvc;

namespace TallerAutomotriz.UI.Controllers
{
    public class RepuestoController : Controller
    {
        public IActionResult Repuesto()
        {
            return View();
        }

        public IActionResult Agregar()
        {
            return View();
        }

        public IActionResult Entregar()
        {
            return View();
        }
    }
}
