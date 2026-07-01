using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Web.Controllers
{
    [Authorize] 
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // M7 will build real dashboard here
            // For now just shows welcome page
            return View();
        }
    }
}