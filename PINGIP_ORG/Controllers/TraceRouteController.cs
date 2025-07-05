using Microsoft.AspNetCore.Mvc;
using PINGIP_ORG.Services;

namespace PINGIP_ORG.Controllers
{
    public class TraceRouteController : Controller
    {
        private readonly ILogger<TraceRouteController> _logger;

        private readonly GlobalTraceRouteIPDictionaryService _globalIPDictionary;

        private readonly TraceRouteService _traceRouteService;

        public TraceRouteController(
            ILogger<TraceRouteController> logger,
            GlobalTraceRouteIPDictionaryService globalIPDictionary,
            TraceRouteService traceRouteService)
        {
            _logger = logger;

            _globalIPDictionary = globalIPDictionary;

            _traceRouteService = traceRouteService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/Home/AJAX/TraceRoute")]
        public async Task<IActionResult> AJAXTraceRoute([FromBody] string ipAddress)
        {
            string remoteIpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(remoteIpAddress))
            {
                return Content("Invalid Request", "text/plain");
            }

            string result = await _traceRouteService.TraceRoute(ipAddress, remoteIpAddress);

            return Content(result, "text/plain");
        }
    }
}
