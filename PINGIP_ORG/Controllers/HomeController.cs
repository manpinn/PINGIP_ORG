using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PINGIP_ORG.Models;
using PINGIP_ORG.Services;

namespace PINGIP_ORG.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/Home/AJAX/PingIP")]
        public ActionResult AJAX_PingIP([FromBody] string ipAdress)
        {
            PingIPService pingIPService = new PingIPService();

            string result = pingIPService.PingIP(ipAdress);

            return Content(result, "text/plain");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
