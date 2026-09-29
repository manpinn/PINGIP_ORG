using Microsoft.AspNetCore.Mvc;
using PINGIP_ORG.Models;
using PINGIP_ORG.Services;

namespace PINGIP_ORG.Controllers
{
    public class DNSController : Controller
    {
        private readonly ILogger<DNSController> _logger;

        private readonly GlobalPingIPDictionaryService _globalIPDictionary;

        private readonly PingIPService _pingIPService;

        public DNSController(
            ILogger<DNSController> logger,
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
        [Route("/Home/AJAX/Whois")]
        public async Task<IActionResult> AJAXWhois([FromBody] string inputIP)
        {
            string? remoteIpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(inputIP) || string.IsNullOrEmpty(remoteIpAddress))
            {
                return Content("Invalid Request", "text/plain");
            }

            string result = await _pingIPService.QueryByIPAddress(inputIP);

            result = "WhoIs " + inputIP + " ?\n\n" + result;

            return Content(result, "text/plain");
        }
    }
}
