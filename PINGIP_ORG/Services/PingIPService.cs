using DnsClient;
using PINGIP_ORG.Common;
using PINGIP_ORG.Enums;
using PINGIP_ORG.Models;
using System.Net.NetworkInformation;
using System.Text;
using Whois.NET;

namespace PINGIP_ORG.Services
{
    public class PingIPService
    {
        private readonly GlobalPingIPDictionaryService _globalIPDictionaryService;

        private readonly ILogger<PingIPService> _logger;

        public PingIPService(GlobalPingIPDictionaryService globalIPDictionaryService, ILogger<PingIPService> logger)
        {
            _globalIPDictionaryService = globalIPDictionaryService;

            _logger = logger;
        }

        public async Task<string> PingIP(PingInput pingInput, string remoteIpAddress)
        {
            var (requestState, message) = _globalIPDictionaryService.RequestFrequencyState(remoteIpAddress, pingInput.ipAddress);

            if (requestState != RequestState.Pass) return message ?? "Request not allowed.";

            int pingCount = 4;            // Default number of pings
            int timeout = 1000;           // Timeout in milliseconds
            int sent = 0, received = 0, lost = 0;
            long minTime = long.MaxValue;
            long maxTime = long.MinValue;
            long totalTime = 0;

            byte[] buffer = new byte[32]; // Default ping buffer

            Ping pingSender = new Ping();

            PingOptions options = new PingOptions() { DontFragment = true };

            StringBuilder result = new StringBuilder();

            result.Append($"Source (Our Server): {GlobalServerIPAddress.ServerIPAddress}").Append("\n");
            result.Append($"Target: {pingInput.ipAddress}").Append("\n").Append("\n");

            result.Append($"Pinged {pingInput.ipAddress} with {buffer.Length} bytes of data:").Append("\n").Append("\n");

            for (int i = 0; i < pingCount; i++)
            {
                try
                {
                    PingReply reply = await pingSender.SendPingAsync(pingInput.ipAddress, timeout, buffer, options);
                    sent++;

                    if (reply != null && reply.Status == IPStatus.Success)
                    {
                        received++;
                        long time = reply.RoundtripTime;
                        minTime = Math.Min(minTime, time);
                        maxTime = Math.Max(maxTime, time);
                        totalTime += time;

                        string? replyBufferLength = null;

                        if (reply.Buffer != null) replyBufferLength = reply.Buffer.Length.ToString();

                        string? replyOptionsTtl = null;

                        if (reply.Options != null) replyOptionsTtl = reply.Options.Ttl.ToString();

                        result.Append($"Reply from {reply.Address}: bytes={replyBufferLength} time={time}ms TTL={replyOptionsTtl}").Append("\n");
                    }
                    else
                    {
                        lost++;
                        result.Append("\n").Append($"Request timed out.").Append("\n");
                    }
                }
                catch (Exception ex)
                {
                    lost++;

                    string errorMessage = ex.Message;

                    if (ex.InnerException != null) errorMessage += "; " + ex.InnerException.Message;

                    result.Append("\n").Append($"Ping failed: {errorMessage}").Append("\n");
                }

                Thread.Sleep(1000); // Wait 1 second between pings
            }

            result.Append("\n").Append($"Ping statistics for {pingInput.ipAddress}:").Append("\n").Append("\n");
            result.Append($"Packets: Sent = {sent}, Received = {received}, Lost = {lost} ({((double)(lost * 100)) / (double)sent}% loss),").Append("\n");

            if (received > 0)
            {
                result.Append("Approximate round trip times in milli-seconds:").Append("\n");
                result.Append($"Minimum = {minTime}ms, Maximum = {maxTime}ms, Average = {totalTime / received}ms").Append("\n");

                _logger.LogInformation($"Ping IP: Pinged {pingInput.ipAddress}: Sent={sent}, Received={received}, Lost={lost}, MinTime={minTime}ms, MaxTime={maxTime}ms, AvgTime={totalTime / received}ms");
            }
            else
            {
                _logger.LogInformation($"Ping IP: Pinged {pingInput.ipAddress}: Sent={sent}, Received={received}, Lost={lost}, MinTime={minTime}ms, MaxTime={maxTime}ms");
            }

            string? whoisResult = null;

            if (pingInput.whois)
            {
                whoisResult = await QueryByIPAddress(pingInput.ipAddress);

                result.Append("\n").Append("WhoIs: ").Append("\n").Append(whoisResult);
            }

            string? nslookpuResult = null;

            if (pingInput.dns)
            {
                nslookpuResult = await NSLookup(pingInput.ipAddress);

                result.Append("\n\n").Append("NSLookUp: ").Append("\n").Append(nslookpuResult);
            }

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
