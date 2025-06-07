using Microsoft.AspNetCore.Mvc;
using PINGIP_ORG.Services;

namespace PINGIP_ORG.Controllers
{
    public class PortCheckController : Controller
    {

        private readonly ILogger<PortCheckController> _logger;

        private readonly GlobalPortCheckIPDictionaryService _globalIPDictionary;

        private readonly PortCheckService _portCheckService;

        public PortCheckController(
            ILogger<PortCheckController> logger,
            GlobalPortCheckIPDictionaryService globalIPDictionary,
            PortCheckService portCheckService)
        {
            _logger = logger;

            _globalIPDictionary = globalIPDictionary;

            _portCheckService = portCheckService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/Home/AJAX/PortCheck")]
        public async Task<IActionResult> AJAXPortCheck([FromBody] PortCheckRequest request)
        {
            string remoteIpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(request.IpAdress) || string.IsNullOrEmpty(remoteIpAddress))
            {
                return Content("Invalid Request", "text/plain");
            }

            string result = await _portCheckService.PortCheck(request.IpAdress, remoteIpAddress, request.Port);

            return Content(result, "text/plain");
        }

        public class PortCheckRequest
        {
            public string IpAdress { get; set; }
            public int Port { get; set; }
        }
    }
}
