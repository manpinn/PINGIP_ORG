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
            var ipCandidates = new List<string>();

            // Check X-Forwarded-For header (may contain multiple IPs)
            var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                ipCandidates.AddRange(forwardedFor.Split(',').Select(ip => ip.Trim()));
            }

            // Fallback: Remote IP address from the connection
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                ipCandidates.Add(remoteIp);
            }

            IPAddress? ipv4 = null;
            IPAddress? ipv6 = null;

            foreach (var ipString in ipCandidates)
            {
                if (IPAddress.TryParse(ipString, out var ipAddress))
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork && ipv4 == null)
                        ipv4 = ipAddress;

                    if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6 && ipv6 == null)
                        ipv6 = ipAddress;
                }

                if (ipv4 != null && ipv6 != null)
                    break; // Stop once both are found
            }

            ViewBag.IPv4 = ipv4?.ToString();
            ViewBag.IPv6 = ipv6?.ToString();

            return View();
        }
    }
}
