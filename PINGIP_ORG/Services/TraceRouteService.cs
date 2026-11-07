using DnsClient;
using PINGIP_ORG.Common;
using PINGIP_ORG.Enums;
using PINGIP_ORG.Models;
using System.Collections;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using Whois.NET;

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

        public async Task<string> TraceRoute(TraceRouteInput traceRouteInput, string remoteIpAddress)
        {
            if (traceRouteInput.ipAddress == null) return "IP address is required";

            var (requestState, message) = _globalIPDictionaryService.RequestFrequencyState(remoteIpAddress, traceRouteInput.ipAddress);

            if (requestState != RequestState.Pass) return message ?? "Unable to trace route";


            traceRouteInput.timeout = traceRouteInput.timeout ?? 10000;           // Timeout in milliseconds
            traceRouteInput.maxhops = traceRouteInput.maxhops ?? 100;

            byte[] buffer = new byte[32]; // Default ping buffer
            Ping pingSender = new Ping();

            StringBuilder result = new StringBuilder();

            result.Append($"Source (Our Server): {GlobalServerIPAddress.ServerIPAddress}").Append("\n");
            result.Append($"Target: {traceRouteInput.ipAddress}").Append("\n").Append("\n");

            result.Append($"Tracing route to {traceRouteInput.ipAddress} over a maximum of {traceRouteInput.maxhops} hops:").Append("\n").Append("\n");

            result.Append($"0 {GlobalServerIPAddress.ServerIPAddress}").Append("\n");

            Stopwatch stopwatch = new Stopwatch();

            for (int ttl = 1; ttl <= traceRouteInput.maxhops; ttl++)
            {
                try
                {
                    var options = new PingOptions(ttl, true);

                    stopwatch.Start();

                    PingReply reply = await pingSender.SendPingAsync(traceRouteInput.ipAddress, traceRouteInput.timeout.Value, buffer, options);

                    stopwatch.Stop();

                    if (reply != null && (reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.Success))
                    {
                        result.Append($"{ttl} {reply.Address} - {stopwatch.ElapsedMilliseconds} ms").Append("\n");

                        string? whoisResult;

                        if (traceRouteInput.whois)
                        {
                            whoisResult = await QueryByIPAddress(reply.Address.ToString());

                            result.Append(whoisResult);
                        }

                        string? nslookpuResult;

                        if (traceRouteInput.dns)
                        {
                            nslookpuResult = await NSLookup(reply.Address.ToString());

                            result.Append(nslookpuResult);
                        }

                        result.Append("\n");

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

            _logger.LogInformation($"TraceRoute IP: TraceRoute {traceRouteInput.ipAddress}");

            return result.ToString();
        }

        public async Task<string> QueryByIPAddress(string ipAddress)
        {
            var options = new WhoisQueryOptions
            {
                Timeout = (int)TimeSpan.FromMilliseconds(5000).TotalMilliseconds,
                Retries = 3,
                RethrowExceptions = false
            };

            var result = await WhoisClient.QueryAsync(ipAddress, options, CancellationToken.None);

            var sb = new StringBuilder();

            sb.Append($"AdressRange: {result.AddressRange.Begin} - {result.AddressRange.End}\n");
            sb.Append($"OrganizationName: {result.OrganizationName}\n");
            sb.Append(string.Join(" > RespondedServers (FQDN) ", result.RespondedServers));

            return sb.ToString();
        }

        public async Task<string> NSLookup(string ipAddress)
        {
            var lookup = new LookupClient();

            var result = await lookup.QueryReverseAsync(new System.Net.IPAddress(System.Net.IPAddress.Parse(ipAddress).GetAddressBytes()));

            var aRecords = result.Answers.ARecords()
                      .Select(x => x.Address)
                      .ToList();

            var cnames = result.Answers.CnameRecords()
                     .Select(x => x.CanonicalName)
                     .ToList();

            var ptrs = result.Answers.PtrRecords()
                               .Select(x => x.PtrDomainName)
                               .ToList();

            var sb = new StringBuilder();

            //string aRecordsList = aRecords.Count > 0
            //    ? string.Join(",\n", aRecords)
            //    : "No A records";

            //string cnamesList = cnames.Count > 0
            //    ? string.Join(",\n", cnames)
            //    : "No CNAMES records";

            string ptrList = ptrs.Count > 0
                ? string.Join(",\n", ptrs)
                : "No PTR records";

            //sb.Append(aRecordsList).Append("\n");

            //sb.Append(cnamesList).Append("\n");

            sb.Append(ptrList).Append("\n");

            return sb.ToString();
        }





    }
}
