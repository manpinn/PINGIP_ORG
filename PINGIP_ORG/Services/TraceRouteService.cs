using PINGIP_ORG.Common;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

namespace PINGIP_ORG.Services
{
    public class TraceRouteService
    {
        private readonly GlobalTraceRouteIPDictionaryService _globalIPDictionary;

        private readonly ILogger<TraceRouteService> _logger;

        public TraceRouteService(GlobalTraceRouteIPDictionaryService globalIPDictionary, ILogger<TraceRouteService> logger)
        {
            _globalIPDictionary = globalIPDictionary;

            _logger = logger;
        }

        public async Task<string> TraceRoute(string ipAddress, string remoteIpAddress)
        {
            if (_globalIPDictionary.TryGet(remoteIpAddress, out DateTime lastPing))
            {
                if (DateTime.Now - lastPing < Globals.minTraceRouteTimeSpan)
                {
                    return $"TraceRoute from your IP-Address is too frequent. Please wait {(int)((Globals.minTraceRouteTimeSpan - (DateTime.Now - lastPing)).TotalSeconds)} seconds.";
                }
                else
                {
                    _globalIPDictionary.AddOrUpdate(remoteIpAddress, DateTime.Now);
                }
            }
            else
            {
                _globalIPDictionary.AddOrUpdate(remoteIpAddress, DateTime.Now);
            }

            int timeout = 5000;           // Timeout in milliseconds
            int maxHops = 100;

            byte[] buffer = new byte[32]; // Default ping buffer
            Ping pingSender = new Ping();

            StringBuilder result = new StringBuilder();

            result.Append($"Source: {GlobalServerIPAddress.ServerIPAddress}").Append("\n");
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
                    string message = ex.Message;

                    if (ex.InnerException != null) message += "; " + ex.InnerException.Message;

                    result.Append($"TraceRoute failed: {message}").Append("\n");
                }

                Thread.Sleep(1000); // Wait 1 second between pings
            }

            _logger.LogInformation($"TraceRoute IP: TraceRoute {ipAddress}");

            return result.ToString();
        }
    }
}
