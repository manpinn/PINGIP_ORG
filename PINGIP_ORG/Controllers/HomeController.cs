using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PINGIP_ORG.Models;
using PINGIP_ORG.Services;

namespace PINGIP_ORG.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly GlobalPingIPDictionaryService _globalIPDictionary;

        private readonly PingIPService _pingIPService;

        public HomeController(
            ILogger<HomeController> logger, 
            GlobalPingIPDictionaryService globalIPDictionary,
            PingIPService pingIPService)
        {
            _logger = logger;

            _globalIPDictionary = globalIPDictionary;

            _pingIPService = pingIPService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/Home/AJAX/PingIP")]
        public async Task<IActionResult> AJAXPingIP([FromBody] string ipAdress)
        {
            string remoteIpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAdress) || string.IsNullOrEmpty(remoteIpAddress))
            {
                return Content("Invalid Request", "text/plain");
            }

            string result = await _pingIPService.PingIP(ipAdress, remoteIpAddress);

            return Content(result, "text/plain");
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
