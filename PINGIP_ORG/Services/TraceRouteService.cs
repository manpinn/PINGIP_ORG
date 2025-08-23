using PINGIP_ORG.Common;
using PINGIP_ORG.Enums;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

namespace PINGIP_ORG.Services
{
    public class TraceRouteService
    {
        private readonly GlobalTraceRouteIPDictionaryService _globalIPDictionaryService;

        private readonly ILogger<TraceRouteService> _logger;

        public TraceRouteService(GlobalTraceRouteIPDictionaryService globalIPDictionaryService, ILogger<TraceRouteService> logger)
        {
            _globalIPDictionaryService = globalIPDictionaryService;

            _logger = logger;
        }

        public async Task<string> TraceRoute(string ipAddress, string remoteIpAddress)
        {
            var(requestState, message) = _globalIPDictionaryService.RequestFrequencyState(remoteIpAddress, ipAddress);

            if (requestState != RequestState.Pass) return message;


            int timeout = 5000;           // Timeout in milliseconds
            int maxHops = 100;

            byte[] buffer = new byte[32]; // Default ping buffer
            Ping pingSender = new Ping();

            StringBuilder result = new StringBuilder();

            result.Append($"Source (Our Server): {GlobalServerIPAddress.ServerIPAddress}").Append("\n");
            result.Append($"Target: {ipAddress}").Append("\n").Append("\n");

            result.Append($"Tracing route to {ipAddress} over a maximum of {maxHops} hops:").Append("\n").Append("\n");

            result.Append($"0 {GlobalServerIPAddress.ServerIPAddress}").Append("\n");

            Stopwatch stopwatch = new Stopwatch();

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                try
                {
                    var options = new PingOptions(ttl, true);

                    stopwatch.Start();

                    PingReply reply = await pingSender.SendPingAsync(ipAddress, timeout, buffer, options);

                    stopwatch.Stop();

                    if (reply != null && (reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.Success))
                    {
                        result.Append($"{ttl} {reply.Address} - {stopwatch.ElapsedMilliseconds} ms").Append("\n");

                        if (reply.Status == IPStatus.Success)
                        {
                            result.Append("\n").Append("Trace complete.");

                            break;
                        }
                    }
                    else if (reply != null)
                    {
                        result.Append($"* ({reply.Status})").Append("\n");
                    }

                    stopwatch.Reset();
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;

                    if (ex.InnerException != null) errorMessage += "; " + ex.InnerException.Message;

                    result.Append($"TraceRoute failed: {errorMessage}").Append("\n");
                }

                Thread.Sleep(1000); // Wait 1 second between pings
            }

            _logger.LogInformation($"TraceRoute IP: TraceRoute {ipAddress}");

            return result.ToString();
        }
    }
}
