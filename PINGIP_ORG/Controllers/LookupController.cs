using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PINGIP_ORG.Models;
using PINGIP_ORG.Services;

namespace PINGIP_ORG.Controllers
{
    public class LookupController : Controller
    {
        private readonly ILogger<LookupController> _logger;

        private readonly GlobalPingIPDictionaryService _globalIPDictionary;

        private readonly PingIPService _pingIPService;

        public LookupController(
            ILogger<LookupController> logger,
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
        [Route("/Home/AJAX/NSLookup")]
        public async Task<IActionResult> AJAXNSLookup([FromBody] PingInput inputIP)
        {
            string? remoteIpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(inputIP.ipAddress) || string.IsNullOrEmpty(remoteIpAddress))
            {
                return Content("Invalid Request", "text/plain");
            }

            string result = string.Empty;

            if (inputIP.isHostname == true)
            {
                result = await _pingIPService.ReverseNSLookup(inputIP.ipAddress);

                result = "NSLookup for " + inputIP + "\n\n" + result;
            }
            else
            {
                result = await _pingIPService.NSLookup(inputIP.ipAddress);

                result = "NSLookup for " + inputIP + "\n\n" + result;
            }

            return Content(result, "text/plain");
        }
    }
}
