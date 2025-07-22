using Microsoft.AspNetCore.Mvc;

namespace TallerAutomotriz.UI.Controllers
{
    public class SolicitudRepuestoController : Controller
    {
        public IActionResult SolicitudRepuesto()
        {
            return View();
        }

        public IActionResult SolicitarRepuesto()
        {
            return View();
        }
    }
}
