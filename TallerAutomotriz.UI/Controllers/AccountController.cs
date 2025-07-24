using Microsoft.AspNetCore.Mvc;

namespace TallerAutomotriz.UI.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Registrarse()
        {
            return View();
        }
    }
}
