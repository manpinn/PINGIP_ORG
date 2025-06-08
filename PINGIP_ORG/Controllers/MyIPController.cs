using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PINGIP_ORG.Services;
using System.Net;
using System.Net.Sockets;

namespace PINGIP_ORG.Controllers
{
    public class MyIPController : Controller
    {
        private readonly ILogger<MyIPController> _logger;

        public MyIPController(ILogger<MyIPController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("/MyIP/AJAX/IP")]
        [EnableCors("AllowPingIpFrontend")]
        public async Task<IActionResult> AJAXIP()
        {
            string remoteIpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            return Content(remoteIpAddress, "text/plain");
        }
    }
}
