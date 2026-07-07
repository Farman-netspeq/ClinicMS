using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
        [Route("Home/Error")]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ViewData["RequestId"] = requestId;
            return View();
        }
    }
}